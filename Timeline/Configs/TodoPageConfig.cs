using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Configs
{
    public class AzureDevOpsAccessInfo
    {
        public string Username { get; set; }
        public string PersonalAccessToken { get; set; }
        public string Organization { get; set; }
        public string Project { get; set; }

    }

    public class TodoPageConfig
    {
        public AzureDevOpsAccessInfo AzureDevOpsAccessInfo { get; set; }
    }
}
