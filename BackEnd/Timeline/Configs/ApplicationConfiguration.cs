using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Timeline.Configs
{
    public static class ApplicationConfiguration
    {
        public const string WorkDirectoryKey = "WorkDirectory";
        public const string DefaultWorkDirectoryName = "timeline";
        public const string DatabaseFileName = "timeline.db";
        public const string DatabaseBackupDirectoryName = "backup";
        public const string FrontEndKey = "FrontEnd";
        public const string DisableAutoBackupKey = "DisableAutoBackup";
        public const string EnableForwardedHeadersKey = "EnableForwardedHeaders";
        public const string ForwardedHeadersAllowedProxyHostsKey = "ForwardedHeadersAllowedProxyHosts";

        public static bool CheckIsValidBoolString(string? value, string configPath, Boolean defaultValue)
        {
            if (value is null)
            {
                return defaultValue;
            }

            var true_strings = new List<string> { "true", "1", "y", "yes", "on" };
            var false_strings = new List<string> { "false", "0", "n", "no", "off" };

            if (true_strings.Contains(value.ToLowerInvariant()))
            {
                return true;
            }
            else if (false_strings.Contains(value.ToLowerInvariant()))
            {
                return false;
            }
            else
            {
                throw new Exception($"Invalid boolean value {value} in config {configPath}.");
            }
        }

        public static bool GetBoolConfig(IConfiguration configuration, string configPath, bool defaultValue)
        {
            var value = configuration.GetValue<string?>(configPath);
            return CheckIsValidBoolString(value, configPath, defaultValue);
        }
    }
}
