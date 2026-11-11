using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.DTOs;
using Claims.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests;

public class ClaimsControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _client = factory.CreateClient();
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    [Fact]
    public async Task Get_claims_returns_a_list()
    {
        var claims = await _client.GetFromJsonAsync<List<ClaimResponse>>("/Claims", Json, _ct);

        Assert.NotNull(claims);

        //TODO: Apart from ensuring 200 OK being returned, what else can be asserted?
    }

    [Fact]
    public async Task Claim_can_be_created_read_and_deleted()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(cover.Id, cover.StartDate, "Hull damage", ClaimType.Collision, 500);

        var created = await _client.PostAsJsonAsync("/Claims", request, Json, _ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var claim = await created.Content.ReadFromJsonAsync<ClaimResponse>(Json, _ct);

        Assert.Equal(claim, await _client.GetFromJsonAsync<ClaimResponse>(created.Headers.Location, Json, _ct));
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/Claims/{claim!.Id}", _ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/Claims/{claim.Id}", _ct)).StatusCode);
    }

    [Fact]
    public async Task Invalid_claim_returns_validation_errors()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(cover.Id, cover.EndDate.AddDays(1), "Hull damage", ClaimType.Collision, 200_000);

        var response = await _client.PostAsJsonAsync("/Claims", request, Json, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json, _ct);
        Assert.Contains(nameof(Claim.DamageCost), problem!.Errors.Keys);
        Assert.Contains(nameof(Claim.Created), problem.Errors.Keys);
    }

    private async Task<CoverResponse> CreateCoverAsync()
    {
        var today = DateTime.UtcNow.Date;
        var response = await _client.PostAsJsonAsync("/Covers", new CreateCoverRequest(today, today.AddDays(30), CoverType.Yacht), Json, _ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CoverResponse>(Json, _ct))!;
    }
}
