using Couchbase;
using Couchbase.KeyValue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcService.Services.Internal
{
	public class LogConfigurationPollingService : BackgroundService
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly ILogConfigurationBucketProvider _bucketProvider;
		private readonly ILogConfigurationManager _logConfigManager;
		private IBucket _bucket;
		private JObject _logConfiguration;

		public LogConfigurationPollingService(ILogConfigurationBucketProvider bucketProvider, ILogger<LogConfigurationPollingService> logger, 
				ILogConfigurationManager logConfigManager, IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_logConfigManager = logConfigManager ?? throw new ArgumentNullException(nameof(logConfigManager));
			_bucketProvider = bucketProvider ?? throw new ArgumentNullException(nameof(bucketProvider));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private string GetCouchbaseDocumentKey() => AppDomain.CurrentDomain.FriendlyName + "_NLogConfig";

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogTrace("Log configuration polling attempt");
				try
				{
					_bucket = _bucket ?? await _bucketProvider.GetBucketAsync();
					ICouchbaseCollection collection = await _bucket.DefaultCollectionAsync();
					IExistsResult existsResult = await collection.ExistsAsync(GetCouchbaseDocumentKey());

					if (existsResult.Exists)
					{
						IGetResult getResult = await collection.GetAsync(GetCouchbaseDocumentKey());
						JObject serverLogConfiguration = getResult.ContentAs<JObject>();

						if (_logConfiguration == null || !JToken.DeepEquals(serverLogConfiguration, _logConfiguration))
						{
							string logConfigJson = serverLogConfiguration.ToString();

							if (_logConfigManager.ApplyNewConfiguration(logConfigJson))
							{
								_logConfiguration = serverLogConfiguration;
								_logger.LogTrace("Log configuration reloaded");
							}
							else
							{
								_logger.LogWarning($"Failed to apply new log configuration. Json = {logConfigJson}");
							}
						}
						else
						{
							_logger.LogTrace("Log configuration is not changed");
						}
					}

					int delayInMs = 1000 * _configuration.GetValue("LogConfigurationPollingIntervalSec", 10);
					await Task.Delay(delayInMs, stoppingToken);
				}
				catch (Exception err)
				{
					_logger.LogError(err, "Failed to get log configuration from DB");
				}
			}
		}
	}
}
