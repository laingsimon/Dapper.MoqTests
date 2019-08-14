using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    internal class ScalarValue : IConvertible
    {
        private readonly bool _isAsync;
        private readonly MethodInfo _method;
        private readonly object[] _parametersArray;
        private readonly MockDatabase _mockDatabase;

        public ScalarValue(bool isAsync, MethodInfo method, object[] parametersArray, MockDatabase mockDatabase)
        {
            _isAsync = isAsync;
            _method = method;
            _parametersArray = parametersArray;
            _mockDatabase = mockDatabase;
        }

        private object Invoke()
        {
            if (_isAsync)
            {
                return TaskHelper.GetResultOfTask(_method.Invoke(_mockDatabase, _parametersArray));
            }

            return _method.Invoke(_mockDatabase, _parametersArray);
        }

        private object Invoke(Type dataType)
        {
            var method = _method.GetGenericMethodDefinition().MakeGenericMethod(dataType);

            if (_isAsync)
            {
                return TaskHelper.GetResultOfTask(method.Invoke(_mockDatabase, _parametersArray));
            }

            return method.Invoke(_mockDatabase, _parametersArray);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (_method.IsGenericMethod)
            {
                return Invoke(conversionType);
            }

            return Invoke();
        }

        private T ToType<T>(IFormatProvider provider)
        {
            var value = ToType(typeof(T), provider);
            if (value == null)
                return default(T);

            return (T)value;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return ToType<bool>(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return ToType<char>(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return ToType<sbyte>(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return ToType<byte>(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return ToType<short>(provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return ToType<ushort>(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return ToType<int>(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return ToType<uint>(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return ToType<long>(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return ToType<ulong>(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return ToType<float>(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return ToType<double>(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return ToType<decimal>(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return ToType<DateTime>(provider);
        }

        public TypeCode GetTypeCode()
        {
            throw new NotSupportedException();
        }
    }
}
