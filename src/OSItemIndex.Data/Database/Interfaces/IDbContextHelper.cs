using static OsItemIndex.Data.Database.DbContextHelper;

namespace OsItemIndex.Data.Database
{
    public interface IDbContextHelper
    {
        DbContextFactory GetFactory();
    }
}
