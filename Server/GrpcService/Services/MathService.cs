using Grpc.Core;
using Example.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GrpcService
{
	public class MathService : Calculator.CalculatorBase
	{
		private readonly ILogger<MathService> _logger;
		public MathService(ILogger<MathService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task<PowerResponse> Power(PowerRequest request, ServerCallContext context)
		{
			_logger.LogTrace($"x={request.X}, y={request.Y}");

			var response = new PowerResponse
			{
				Result = Math.Pow(request.X, request.Y)
			};

			_logger.LogInformation($"Power result={response.Result}");

			return Task.FromResult(response);
		}
	}
}
