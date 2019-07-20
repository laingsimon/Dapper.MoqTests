using System;

namespace Dapper.MoqTests
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class ParameterTypeAttribute : Attribute
    {
        public ParameterTypeAttribute(ParameterType type)
        {
            Type = type;
        }

        public ParameterType Type { get; }
    }
}
