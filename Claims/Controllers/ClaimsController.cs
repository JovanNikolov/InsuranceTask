using Claims.Auditing;
using Claims.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;


namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly ClaimsContext _claimsContext;
        private readonly Auditer _auditer;
        private readonly ClaimValidator _validator;

        public ClaimsController(ILogger<ClaimsController> logger, ClaimsContext claimsContext, AuditContext auditContext, ClaimValidator validator)
        {
            _logger = logger;
            _claimsContext = claimsContext;
            _auditer = new Auditer(auditContext);
            _validator = validator;
        }

        [HttpGet]
        public async Task<IEnumerable<Claim>> GetAsync()
        {
            return await _claimsContext.GetClaimsAsync();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(Claim claim, CancellationToken cancellationToken)
        {
            var cover = await _claimsContext.Covers.SingleOrDefaultAsync(c => c.Id == claim.CoverId, cancellationToken);
            var errors = _validator.Validate(claim, cover);
            if (errors.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(errors));

            claim.Id = Guid.NewGuid().ToString();
            await _claimsContext.AddItemAsync(claim);
            _auditer.AuditClaim(claim.Id, "POST");
            return Ok(claim);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            _auditer.AuditClaim(id, "DELETE");
            await _claimsContext.DeleteItemAsync(id);
        }

        [HttpGet("{id}")]
        public async Task<Claim> GetAsync(string id)
        {
            return await _claimsContext.GetClaimAsync(id);
        }
    }

    public class ClaimsContext : DbContext
    {

        private DbSet<Claim> Claims { get; init; }
        public DbSet<Cover>  Covers { get; init; }

        public ClaimsContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Claim>().ToCollection("claims");
            modelBuilder.Entity<Cover>().ToCollection("covers");
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            return await Claims.ToListAsync();
        }

        public async Task<Claim> GetClaimAsync(string id)
        {
            return await Claims
                .Where(claim => claim.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task AddItemAsync(Claim item)
        {
            Claims.Add(item);
            await SaveChangesAsync();
        }

        public async Task DeleteItemAsync(string id)
        {
            var claim = await GetClaimAsync(id);
            if (claim is not null)
            {
                Claims.Remove(claim);
                await SaveChangesAsync();
            }
        }
    }
}
