using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GrpcService.Services.Internal
{
	public class LogConfigurationManager : ILogConfigurationManager
	{
		private readonly ILogger _logger;

		public LogConfigurationManager(ILogger<LogConfigurationManager> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public bool ApplyNewConfiguration(string jsonConfinuration)
		{
			if (string.IsNullOrWhiteSpace(jsonConfinuration))
			{
				throw new ArgumentException($"'{nameof(jsonConfinuration)}' cannot be null or whitespace.", nameof(jsonConfinuration));
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonConfinuration)))
			{
				IConfigurationRoot configuration;
				try
				{
					configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
				}
				catch (Exception err)
				{
					_logger.LogWarning(err, $"Faild to build JSON configuration. Json = {jsonConfinuration}");
					return false;
				}

				try
				{
					LogManager.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));
				}
				catch (Exception err)
				{
					_logger.LogWarning(err, $"Faild to apply NLog configuration. Json = {jsonConfinuration}");
					return false;
				}

				return true;
			}
		}
	}
}
