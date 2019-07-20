using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace Dapper.MoqTests
{
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    internal class MockDbParameterCollection : DbParameterCollection, IEquatable<MockDbParameterCollection>, IEnumerable<MockDbParameter>
    {
        public static readonly object Any = new object();

        private readonly List<MockDbParameter> _parameters = new List<MockDbParameter>();

        public MockDbParameterCollection()
        { }

        public MockDbParameterCollection(object parameters)
        {
            if (ReferenceEquals(parameters, Any) || parameters is MockDbParameterCollection)
                throw new ArgumentException("Should not create this type in this way");

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var dbParameters = from property in parameters.GetType().GetProperties()
                             let value = property.GetValue(parameters, null)
                             select new MockDbParameter { ParameterName = property.Name, Value = value };

            foreach (var dbParameter in dbParameters)
                _parameters.Add(dbParameter);
        }

        public override int Count => _parameters.Count;
        public override object SyncRoot => _parameters;

        public override bool Contains(string parameterName)
        {
            return _parameters.Any(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        public override int IndexOf(string parameterName)
        {
            var parameter = GetParameter(parameterName);
            return parameter != null
                ? IndexOf(parameter)
                : -1;
        }

        public override void RemoveAt(string parameterName)
        {
            _parameters.RemoveAll(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        public bool Equals(MockDbParameterCollection executedParameters)
        {
            if (executedParameters == null)
                return false;

            foreach (var parameter in _parameters)
            {
                var executedParameter = (MockDbParameter)executedParameters.GetParameter(parameter.ParameterName);
                if (executedParameter == null)
                {
                    Trace.TraceWarning($"Parameter {parameter.ParameterName} could not be found");
                    return false;
                }

                if (!ValuesMatch(executedParameter, parameter))
                {
                    Trace.TraceWarning($"Parameter {parameter.ParameterName} has a different value, expected {parameter.Value} was {executedParameter.Value}");
                    return false;
                }
            }

            foreach (MockDbParameter executedParameter in executedParameters)
            {
                var parameter = (MockDbParameter)GetParameter(executedParameter.ParameterName);
                if (parameter == null)
                {
                    Trace.TraceWarning($"Parameter {executedParameter.ParameterName} was not expected");
                    return false;
                }

                if (!ValuesMatch(executedParameter, parameter))
                {
                    Trace.TraceWarning($"Parameter {executedParameter.ParameterName} has a different value, expected {parameter.Value} was {executedParameter.Value}");
                    return false;
                }
            }

            return true;
        }

        private bool ValuesMatch(MockDbParameter param1, MockDbParameter param2)
        {
            return param1.Value.Equals(param2.Value);
        }

        public override string ToString()
        {
            return string.Join(", ", _parameters.Select(p => $"{p.ParameterName} = {p.Value}"));
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MockDbParameterCollection ?? new MockDbParameterCollection(obj));
        }

        IEnumerator<MockDbParameter> IEnumerable<MockDbParameter>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public override int Add(object value)
        {
            var parameter = (MockDbParameter)value;
            _parameters.Add(parameter);
            return _parameters.IndexOf(parameter);
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
                Add(value);
        }

        public override bool Contains(object value)
        {
            return _parameters.Contains(value);
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public override void Clear()
        {
            _parameters.Clear();
        }

        public override IEnumerator GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            return _parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _parameters.SingleOrDefault(p => p.ParameterName == parameterName); //TODO: Throw here if not found?
        }

        public override int IndexOf(object value)
        {
            return _parameters.IndexOf((MockDbParameter)value);
        }

        public override void Insert(int index, object value)
        {
            _parameters.Insert(index, (MockDbParameter)value);
        }

        public override void Remove(object value)
        {
            _parameters.Remove((MockDbParameter)value);
        }

        public override void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameters[index] = (MockDbParameter)value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = _parameters.FindIndex(p => p.ParameterName == parameterName);
            var parameter = (MockDbParameter)value;

            if (index == -1)
                _parameters.Add(parameter);
            else
                _parameters[index] = parameter;
        }
    }
}