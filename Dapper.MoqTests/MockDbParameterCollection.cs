using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace Dapper.MoqTests
{
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    internal class MockDbParameterCollection : DbParameterCollection
    {
        public static readonly object Any = new object();

        private readonly List<MockDbParameter> _parameters = new List<MockDbParameter>();

        public MockDbParameterCollection()
        { }

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