# Welcome to Timeline!

[![Build Status](https://dev.azure.com/crupest-web/Timeline/_apis/build/status/crupest.Timeline?branchName=master)](https://dev.azure.com/crupest-web/Timeline/_build/latest?definitionId=7&branchName=master)

This is the first web app back-end of [me](https://github.com/crupest).

It is written in C# and built with [Asp.Net Core](https://github.com/aspnet/AspNetCore).

The final product is hosting on my [Tencent Cloud](https://cloud.tencent.com/) Cloud Virtual Machine on https://api.crupest.xyz.

Feel free to comment by opening an issue.

`tools/open-code` file is a simple _bash_ script that fixes the problem that _OminiSharp_ in C# extension on vscode can't work using _dotnet_ in Arch official package repository on Arch Linux. See [this page](https://bugs.archlinux.org/task/60903).

## about database configuration

You can configure two database, one for development, other for production, at the same time in the configuration file (better in user secret file):

```json
{
  "DatabaseConfig": {
    "UseDevelopment": true,
    "ConnectionString": "The secret production database string. (MySql)",
    "DevelopmentConnectionString": "The private development database string. (Sqlite)"
  }
}
```

Set `UseDevelopment` to `true` to use `DevelopmentConnectionString` as sqlite connection string. Set `UseDevelopment` to `false` to use `ConnectionString` as mysql connection string. Usually you set it to `true` in development mode and `false` in production mode. But you may have to use production database to do migration in development so you may also set it to `false` in development to do that. The migrations for two database are different because they use different providers and remember to use it to switch to right database when do migration.

If you have better solution to deal with database, please don't hesitate to tell me. 😛
