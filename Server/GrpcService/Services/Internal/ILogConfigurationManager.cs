
namespace GrpcService.Services.Internal
{
	public interface ILogConfigurationManager
	{
		bool ApplyNewConfiguration(string jsonConfinuration);
	}
}
