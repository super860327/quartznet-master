namespace Quartz.Spi
{
    public interface IThreadExecutor
    {
        void Execute(QuartzThread thread);

        void Initialize();
    }
}