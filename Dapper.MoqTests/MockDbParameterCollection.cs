namespace Dapper.MoqTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    internal class MockDbParameterCollection : List<MockDbParameter>, IDataParameterCollection, IEquatable<MockDbParameterCollection>
    {
        public static readonly MockDbParameterCollection Any = new MockDbParameterCollection();

        public MockDbParameterCollection()
        { }

        public MockDbParameterCollection(object parameters)
        {
            var properties = from property in parameters.GetType().GetProperties()
                let value = property.GetValue(parameters, null)
                select new MockDbParameter { ParameterName = property.Name, Value = value};

            AddRange(properties);
        }

        public bool Contains(string parameterName)
        {
            return this.Any(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        public int IndexOf(string parameterName)
        {
            var parameter = (MockDbParameter)this[parameterName];
            return parameter != null
                ? IndexOf(parameter)
                : -1;
        }

        public void RemoveAt(string parameterName)
        {
            RemoveAll(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        public object this[string parameterName]
        {
            get { return this.FirstOrDefault(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase)); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Equals(MockDbParameterCollection executedParameters)
        {
            if (executedParameters == null)
                return false;

            foreach (var parameter in this)
            {
                var executedParameter = (MockDbParameter)executedParameters[parameter.ParameterName];
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

            foreach (var executedParameter in executedParameters)
            {
                var parameter = (MockDbParameter)this[executedParameter.ParameterName];
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
            if (ReferenceEquals(this, Any))
                return "<ANY>";

            return string.Join(", ", this.Select(p => $"{p.ParameterName} = {p.Value}"));
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MockDbParameterCollection);
        }

        public static MockDbParameterCollection Create(object parameters)
        {
            return ReferenceEquals(parameters, MockDbConnection.Any)
                ? Any
                : new MockDbParameterCollection(parameters);
        }
    }
}