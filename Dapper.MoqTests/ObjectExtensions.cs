using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Dapper.MoqTests
{
    internal static class ObjectExtensions
    {
        public static IDataReader GetDataReader(this object value)
        {
            if (value == null || IsPrimitiveType(value.GetType()))
                return new DataTableReader(GetDataTableForPrimativeType(value));

            var enumerable = value as IEnumerable;
            if (enumerable != null)
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

        private static DataTable GetDataTableForPrimativeType(object value)
        {
            return new DataTable
            {
                Columns =
                {
                    { "Column0", value?.GetType() ?? typeof(object) }
                },
                Rows = { value }
            };
        }

        private static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(Guid);
        }
    }
}
