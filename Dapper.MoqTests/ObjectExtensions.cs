using System;
using System.Data;
using System.Linq;

namespace Dapper.MoqTests
{
    public static class ObjectExtensions
    {
        public static IDataReader GetDataReader(this object value)
        {
            if (value == null || IsPrimitiveType(value.GetType()))
                return new DataTableReader(GetDataTableForPrimativeType(value));

            if (value.GetType().IsArray)
                return new DataTableReader(GetDataTableForArray((Array)value));

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

        private static DataTable GetDataTableForArray(Array value)
        {
            var properties = value.GetType().GetElementType()
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
