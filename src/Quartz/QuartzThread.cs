using System.Globalization;
using System.Threading;

namespace Quartz
{
    public interface IThreadRunnable
    {
        void Run();
    }

    public abstract class QuartzThread : IThreadRunnable
    {
        private readonly Thread thread;

        protected QuartzThread()
        {
            thread = new Thread(Run);
        }

        protected QuartzThread(string name)
        {
            thread = new Thread(Run);
            Name = name;
        }

        public virtual void Run()
        {
        }

        public void Start()
        {
            thread.Start();
        }

        protected void Interrupt()
        {
            thread.Interrupt();
        }

        public string Name
        {
            get { return thread.Name; }
            protected set
            {
                thread.Name = value;
            }
        }

        protected ThreadPriority Priority
        {
            get { return thread.Priority; }
            set { thread.Priority = value; }
        }

        protected bool IsBackground
        {
            set { thread.IsBackground = value; }
        }

        public void Join()
        {
            thread.Join();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Thread[{0},{1},]", Name, Priority);
        }
    }
}
