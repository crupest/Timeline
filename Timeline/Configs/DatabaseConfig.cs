namespace Timeline.Configs
{
    public class DatabaseConfig
    {
        public bool UseDevelopment { get; set; } = false;

        public string ConnectionString { get; set; } = default!;

        public string DevelopmentConnectionString { get; set; } = default!;
    }
}
