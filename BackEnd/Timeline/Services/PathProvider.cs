using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Timeline.Configs;

namespace Timeline.Services
{
    public interface IPathProvider
    {
        public string GetWorkDirectory();
        public string GetDatabaseFilePath();
        public string GetDatabaseBackupDirectory();
    }

    public class PathProvider : IPathProvider
    {
        private readonly IConfiguration _configuration;

        private readonly string _workDirectory;

        public static string GetDefaultWorkDirectory()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ApplicationConfiguration.DefaultWorkDirectoryName);
        }

        public PathProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _workDirectory = configuration.GetValue<string?>(ApplicationConfiguration.WorkDirectoryKey) ?? GetDefaultWorkDirectory();
        }

        public string GetWorkDirectory()
        {
            return _workDirectory;
        }

        public string GetDatabaseFilePath()
        {
            return Path.Combine(_workDirectory, ApplicationConfiguration.DatabaseFileName);
        }

        public string GetDatabaseBackupDirectory()
        {
            return Path.Combine(_workDirectory, ApplicationConfiguration.DatabaseBackupDirectoryName);
        }
    }
}
