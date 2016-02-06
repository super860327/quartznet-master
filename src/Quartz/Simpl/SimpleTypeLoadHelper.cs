using System;
using System.IO;

using Quartz.Spi;

namespace Quartz.Simpl
{
    public class SimpleTypeLoadHelper : ITypeLoadHelper
    {
        public virtual void Initialize()
        {
        }

        public virtual Type LoadType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return Type.GetType(name, true);
        }

        public virtual Uri GetResource(string name)
        {
            return null;
        }

        public virtual Stream GetResourceAsStream(string name)
        {
            return null;
        }
    }
}