using Microsoft.Extensions.Configuration;
using System.IO;
using Timeline.Configs;

namespace Timeline.Services
{
    public interface IPathProvider
    {
        public string GetWorkingDirectory();
        public string GetDatabaseFilePath();
    }

    public class PathProvider : IPathProvider
    {
        private readonly IConfiguration _configuration;

        private readonly string _workingDirectory;


        public PathProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _workingDirectory = configuration.GetValue<string?>(ApplicationConfiguration.WorkDirKey) ?? ApplicationConfiguration.DefaultWorkDir;
        }

        public string GetWorkingDirectory()
        {
            return _workingDirectory;
        }

        public string GetDatabaseFilePath()
        {
            return Path.Combine(_workingDirectory, ApplicationConfiguration.DatabaseFileName);
        }
    }
}
