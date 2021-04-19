using Microsoft.EntityFrameworkCore;

namespace OsItemIndex.Data.Database
{
    public class OsItemIndexDbContext : DbContext
    {
        public DbSet<OsrsBoxItem> Items { get; set; }
        public DbSet<RealtimePrice> PricesRealtime { get; set; }

        public OsItemIndexDbContext(DbContextOptions<OsItemIndexDbContext> options) : base(options) { }
    }
}
