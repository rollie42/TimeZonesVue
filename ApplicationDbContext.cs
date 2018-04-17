using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TimeZonesVue
{
    //public class ApplicationDbContext : IdentityDbContext
    //{
    //    public DbSet<UserDefinedTimeZone> TimeZones { get; set; }

    //    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    //    : base(options)
    //    {
    //    }

    //    protected override void OnModelCreating(ModelBuilder builder)
    //    {
    //        base.OnModelCreating(builder);
    //        // Customize the ASP.NET Identity model and override the defaults if needed.
    //        // For example, you can rename the ASP.NET Identity table names and more.
    //        // Add your customizations after calling base.OnModelCreating(builder);
    //    }
    //}

    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext([FromServices]IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration["ConnectionString"]);
        }

        public DbSet<UserDefinedTimeZone> TimeZones { get; set; }
    }
}