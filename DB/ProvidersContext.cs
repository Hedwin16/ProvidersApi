using Microsoft.EntityFrameworkCore;

namespace DB
{
    public class ProvidersContext : DbContext
    {
        public ProvidersContext(DbContextOptions<ProvidersContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Provider> Providers { get; set; } 
        public DbSet<Enterprise> Enterprises { get; set; }
        public DbSet<Transaction> Transactions { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

    }
}