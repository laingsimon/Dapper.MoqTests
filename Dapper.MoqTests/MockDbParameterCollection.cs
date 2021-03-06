﻿using System;
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
        private readonly List<DbParameter> _parameters = new List<DbParameter>();
        private readonly Settings _settings;

        public MockDbParameterCollection(Settings settings)
        {
            _settings = settings;
        }

        public override int Count => _parameters.Count;
        public override object SyncRoot => _parameters;

        public override bool Contains(string parameterName)
        {
            var comparer = _settings.SqlParametersComparer;
            return _parameters.Any(p => comparer.Equals(p.ParameterName, parameterName));
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
            var comparer = _settings.SqlParametersComparer;
            _parameters.RemoveAll(p => comparer.Equals(p.ParameterName, parameterName));
        }

        public override int Add(object value)
        {
            var parameter = (DbParameter)value;
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
            var comparer = _settings.SqlParametersComparer;
            return _parameters.SingleOrDefault(p => comparer.Equals(p.ParameterName, parameterName)); //TODO: Throw here if not found?
        }

        public override int IndexOf(object value)
        {
            return _parameters.IndexOf((DbParameter)value);
        }

        public override void Insert(int index, object value)
        {
            _parameters.Insert(index, (DbParameter)value);
        }

        public override void Remove(object value)
        {
            _parameters.Remove((DbParameter)value);
        }

        public override void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameters[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var comparer = _settings.SqlParametersComparer;
            var index = _parameters.FindIndex(p => comparer.Equals(p.ParameterName, parameterName));

            if (index == -1)
                _parameters.Add(value);
            else
                _parameters[index] = value;
        }
    }
}