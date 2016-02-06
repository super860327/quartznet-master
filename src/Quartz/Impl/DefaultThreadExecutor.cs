using Quartz.Spi;

namespace Quartz.Impl
{
    public class DefaultThreadExecutor : IThreadExecutor
    {
        public void Execute(QuartzThread thread)
        {
            thread.Start();
        }

        public void Initialize()
        {
        }
    }
}