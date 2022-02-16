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
                .Select(p => new { Property = p, NullablePrimitiveType = NullablePrimitiveType(p.PropertyType) })
                .Where(p => p.NullablePrimitiveType != null || IsPrimitiveType(p.Property.PropertyType))
                .ToArray();
            var dataTable = new DataTable();
            foreach (var property in properties)
            {
                if (property.NullablePrimitiveType != null)
                {
                    var column = new DataColumn(property.Property.Name, property.NullablePrimitiveType);
                    column.AllowDBNull = true;
                    dataTable.Columns.Add(column);
                    break;
                }

                dataTable.Columns.Add(property.Property.Name, property.Property.PropertyType);
            }

            var rowValues = properties.Select(p => p.Property.GetValue(value)).ToArray();
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
                .Select(p => new { Property = p, NullablePrimitiveType = NullablePrimitiveType(p.PropertyType) })
                .Where(p => p.NullablePrimitiveType != null || IsPrimitiveType(p.Property.PropertyType))
                .ToArray();
            var dataTable = new DataTable();
            foreach (var property in properties)
            {
                if (property.NullablePrimitiveType != null)
                {
                    var column = new DataColumn(property.Property.Name, property.NullablePrimitiveType);
                    column.AllowDBNull = true;
                    dataTable.Columns.Add(column);
                    break;
                }

                dataTable.Columns.Add(property.Property.Name, property.Property.PropertyType);
            }

            foreach (var row in value)
            {
                var rowValues = properties.Select(p => p.Property.GetValue(row)).ToArray();
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
        private static Type NullablePrimitiveType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType != null && IsPrimitiveType(underlyingType))
            {
                return underlyingType;
            }

            return null;
        }
    }
}
