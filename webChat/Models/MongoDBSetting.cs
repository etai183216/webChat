namespace webChat.Models
{
    public class MongoDBSetting
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string ChatCollectionName { get; set; } = null!;

        public string UserCollectionName { get; set; } = null!;
    }
}
