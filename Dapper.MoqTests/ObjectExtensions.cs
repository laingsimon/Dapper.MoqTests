using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Dapper.MoqTests
{
    internal static class ObjectExtensions
    {
        private static readonly Type[] PrimitiveDataTypes =
        {
            typeof(string),
            typeof(DateTime),
            typeof(Guid)
        };

        public static IDataReader GetDataReader(this object value)
        {
            if (value == null || IsPrimitiveType(value.GetType()))
                return new DataTableReader(GetPrimitiveTypeDataTable(new[] { value }, value?.GetType() ?? typeof(object)));

            if (value is IEnumerable enumerable)
                return new DataTableReader(GetDataTableForArray(enumerable));

            var properties = value.GetType()
                .GetProperties()
                .Where(p => IsPrimitiveType(p.PropertyType))
                .ToArray();
            var dataTable = new DataTable();
            foreach (var property in properties)
                dataTable.Columns.Add(property.Name, property.PropertyType);

            var rowValues = properties.Select(p => p.GetValue(value)).ToArray();
            dataTable.Rows.Add(rowValues);

            return new DataTableReader(dataTable);
        }

        private static DataTable GetDataTableForArray(IEnumerable value)
        {
            var collectionType = value.GetType();
            var elementType = collectionType.GetElementType()
                ?? collectionType.GetGenericArguments().FirstOrDefault()
                ?? typeof(object);

            if (IsPrimitiveType(elementType))
                return GetPrimitiveTypeDataTable(value, elementType);

            var properties = elementType
                .GetProperties()
                .Where(p => IsPrimitiveType(p.PropertyType))
                .ToArray();
            var dataTable = new DataTable();
            foreach (var property in properties)
                dataTable.Columns.Add(property.Name, property.PropertyType);

            foreach (var row in value)
            {
                var rowValues = properties.Select(p => p.GetValue(row)).ToArray();
                dataTable.Rows.Add(rowValues);
            }

            return dataTable;
        }

        private static DataTable GetPrimitiveTypeDataTable(IEnumerable value, Type elementType)
        {
            var dataTable = new DataTable
            {
                Columns =
                {
                    { "Column0", elementType }
                }
            };

            foreach (var row in value)
                dataTable.Rows.Add(row);

            return dataTable;
        }

        private static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || PrimitiveDataTypes.Contains(type);
        }
    }
}
