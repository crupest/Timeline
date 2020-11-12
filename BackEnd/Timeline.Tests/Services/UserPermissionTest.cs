using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserPermissionTest : DatabaseBasedTest
    {
        private UserPermissionService _service;

        public UserPermissionTest()
        {

        }

        protected override void OnDatabaseCreated()
        {
            _service = new UserPermissionService(Database);
        }


    }
}
