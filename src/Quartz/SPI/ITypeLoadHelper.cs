using System;
using System.IO;

namespace Quartz.Spi
{
    public interface ITypeLoadHelper
    {
        void Initialize();

        Type LoadType(string name);

        Uri GetResource(string name);

        Stream GetResourceAsStream(string name);
    }
}