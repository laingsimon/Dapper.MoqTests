using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.MoqTests
{
    internal class ParametersComparer : IParametersComparer
    {
        private readonly IEqualityComparer<string> _nameComparer;

        public ParametersComparer(IEqualityComparer<string> nameComparer)
        {
            _nameComparer = nameComparer;
        }

        public new bool Equals(object x, object y)
        {
            var xDict = GetDict(x);
            var yDict = GetDict(y);

            return Equals(xDict, yDict);
        }

        public bool Equals(string x, string y)
        {
            return _nameComparer.Equals(x, y);
        }

        public int GetHashCode(string obj)
        {
            return _nameComparer.GetHashCode(obj);
        }

        private bool Equals(IDictionary<string, object> xDict, IDictionary<string, object> yDict)
        {
            return xDict.Keys.SequenceEqual(yDict.Keys)
                && xDict.Keys.All(key => ParameterEquals(xDict[key], yDict[key]));
        }

        private bool ParameterEquals(object x, object y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (x.GetType().IsArray)
                return ((IEnumerable)x).Cast<object>().SequenceEqual(((IEnumerable)y).Cast<object>());

            return x.Equals(y);
        }

        private IDictionary<string, object> GetDict(object parameters)
        {
            if (parameters == null)
                return new Dictionary<string, object>();

            return parameters.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters, null));
        }

        public int GetHashCode(object obj)
        {
            return GetDict(obj).GetHashCode();
        }
    }
}
