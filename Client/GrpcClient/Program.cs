using Example.Tools;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
	class Program
	{
		private const int RandomMaxNumber = 100;

		static async Task Main()
		{
			using var channel = GrpcChannel.ForAddress("https://localhost:5001");
			var client = new Calculator.CalculatorClient(channel);

			var random = new Random();
			var request = new PowerRequest { X = random.Next(RandomMaxNumber), Y = random.Next(RandomMaxNumber) };
			PowerResponse response = await client.PowerAsync(request);

			Console.WriteLine($"{request.X} ^ {request.Y} = { response.Result }");
		}
	}
}
