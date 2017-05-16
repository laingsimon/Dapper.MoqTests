using System;
using System.Reflection;

namespace Dapper.MoqTests.Samples
{
    internal static class TestExtensions
    {
        public static T Prop<T>(this object instance, string propertyName)
        {
            if (instance == null)
                return default(T);

            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (property == null)
                throw new MissingMemberException(instance.GetType().FullName, propertyName);

            return (T)property.GetValue(instance);
        }
    }
}
