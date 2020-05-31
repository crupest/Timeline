using Microsoft.Extensions.Configuration;
using System.IO;

namespace Timeline.Services
{
    public interface IPathProvider
    {
        public string GetWorkingDirectory();
        public string GetDatabaseFilePath();
    }

    public class PathProvider : IPathProvider
    {
        const string DatabaseFileName = "timeline.db";

        private readonly IConfiguration _configuration;

        private readonly string _workingDirectory;


        public PathProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _workingDirectory = configuration.GetValue<string?>("WorkDir") ?? "/timeline";
        }

        public string GetWorkingDirectory()
        {
            return _workingDirectory;
        }

        public string GetDatabaseFilePath()
        {
            return Path.Combine(_workingDirectory, DatabaseFileName);
        }
    }
}
