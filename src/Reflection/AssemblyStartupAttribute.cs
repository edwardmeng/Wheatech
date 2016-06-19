using System;

namespace Wheatech.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyStartupAttribute : Attribute
    {
        public AssemblyStartupAttribute(Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }
            StartupType = startupType;
        }

        public Type StartupType { get; private set; }
    }
}
