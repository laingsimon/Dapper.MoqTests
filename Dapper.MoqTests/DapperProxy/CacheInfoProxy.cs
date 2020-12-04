using System;
using System.Data;
using System.Reflection;

namespace Dapper.MoqTests
{
    public class CacheInfoProxy
    {
        private readonly object cacheInfo;

        public CacheInfoProxy(object cacheInfo)
        {
            this.cacheInfo = cacheInfo;
        }

        public object Deserializer => GetPropertyValue<object>(nameof(Deserializer));
        public object OtherDeserializers => GetPropertyValue<Func<IDataReader, object>[]>(nameof(OtherDeserializers));
        public object ParamReader => GetPropertyValue<Action<IDbCommand, object>>(nameof(ParamReader));
        public int hitCount => GetFieldValue<int>(nameof(hitCount));

        private T GetPropertyValue<T>(string propertyName)
        {
            var property = cacheInfo.GetType().GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property {propertyName} not found in {cacheInfo.GetType().FullName}");
            }

            var value = property.GetValue(cacheInfo);
            return (T)value;
        }

        private T GetFieldValue<T>(string fieldName)
        {
            var property = cacheInfo.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentException($"Field {fieldName} not found in {cacheInfo.GetType().FullName}");
            }

            var value = property.GetValue(cacheInfo);
            return (T)value;
        }
    }
}
