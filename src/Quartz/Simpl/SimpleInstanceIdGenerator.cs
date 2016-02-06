using Quartz.Spi;

namespace Quartz.Simpl
{
    public class SimpleInstanceIdGenerator : HostNameBasedIdGenerator
    {
        private const int HostNameMaxLength = IdMaxLength - 20;

        public override string GenerateInstanceId()
        {
            string hostName = GetHostName(HostNameMaxLength);
            return hostName + SystemTime.UtcNow().Ticks;
        }
    }
}