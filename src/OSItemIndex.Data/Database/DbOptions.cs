namespace OsItemIndex.Data.Database
{
    public class DbOptions
    {
        public string DbConnectionString { get; set; }
        public int PoolSize { get; set; } = 128;
    }
}
