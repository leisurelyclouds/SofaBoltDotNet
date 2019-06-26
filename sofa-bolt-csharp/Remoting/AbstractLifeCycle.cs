namespace Remoting
{
    public abstract class AbstractLifeCycle : LifeCycle
    {
        private volatile bool isStarted = false;

        public virtual void startup()
        {
            if (!isStarted)
            {
                isStarted = true;
                return;
            }

            throw new LifeCycleException("this component has started");
        }

        public virtual void shutdown()
        {
            if (isStarted)
            {
                isStarted = false;
                return;
            }

            throw new LifeCycleException("this component has closed");
        }

        public virtual bool Started
        {
            get
            {
                return isStarted;
            }
        }
    }
}