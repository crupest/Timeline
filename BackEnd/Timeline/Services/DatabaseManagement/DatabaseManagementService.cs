﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Timeline.Configs;
using Timeline.Entities;

namespace Timeline.Services.DatabaseManagement
{
    public class DatabaseManagementService : IHostedService
    {
        private readonly ILogger<DatabaseManagementService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _disableAutoBackup;

        public DatabaseManagementService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DatabaseManagementService> logger)
        {
            _serviceProvider = serviceProvider;
            _disableAutoBackup = ApplicationConfiguration.GetBoolConfig(configuration, ApplicationConfiguration.DisableAutoBackupKey, false);
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            var backupService = provider.GetRequiredService<IDatabaseBackupService>();
            var database = provider.GetRequiredService<DatabaseContext>();
            var customMigrator = provider.GetRequiredService<IDatabaseCustomMigrator>();


            if (!_disableAutoBackup)
            {
                await backupService.BackupAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("Auto backup is disabled. Please backup your database manually.");
            }
            await database.Database.MigrateAsync(cancellationToken);
            await customMigrator.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
