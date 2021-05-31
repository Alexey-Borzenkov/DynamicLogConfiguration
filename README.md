# Dynamic Log Configuration

This project is a demo of reconfiguring the NLog logging on the fly when log configuration stored in the NoSQL document database (Couchbase).
The demo contains client that calls server using gRPC for doing simple math operation and it is using .NET 5.0.

## Setup and configure
1) Install on premise or connect to the Cloud Couchbase Server.
2) In the `appsettings.json` update the `Couchbase` section using your credentials.
3) In the Couchbase Server create a bucket with `log-configuration` name.
4) In the `log-configuration` bucket create a document with a `GrpcService_NLogConfig` id and JSON NLog configuration.
```json
{
  "NLog": {
    "autoReload": true,
    "throwConfigExceptions": true,
    "internalLogLevel": "Info",
    "internalLogFile": "log\nlog-internal.log",
    "extensions": {
      "NLog.Web.AspNetCore": {
        "assembly": "NLog.Web.AspNetCore"
      }
    },
    "targets": {
      "async": true,
      "logFile": {
        "type": "File",
        "fileName": "log/nlog-${shortdate}.log"
      },
      "logConsole": {
        "type": "Console"
      }
    },
    "rules": [
      {
        "logger": "*",
        "minLevel": "Trace",
        "writeTo": "logConsole"
      },
      {
        "logger": "*",
        "minLevel": "Trace",
        "writeTo": "logFile"
      }
    ]
  }
}
```
5) Build the solution.

## How to use
- Start server from VS or console.
- Start client from console.
- Default NLog configuration writes logs to server console out and to the `log` subdirectory. 
- You can update NLog configuration document in the Couchbase Server and changes will be updated in 10 seconds. This can be change by modifying the `LogConfigurationPollingIntervalSec` parameter in the `appsettings.json` file
- Server uses polling approach, but you can modify it to using `On demand` approach if there is a lack of server resources.
