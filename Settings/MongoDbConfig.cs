namespace MongoWebGallery.Settings
{
    public class MongoDbConfig
    {
        public string Name { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Port { get; set; } = null!;

        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }
}
