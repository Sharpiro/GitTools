using System.Collections.Generic;
using System.Data;

namespace System.Linq
{
    public static class Linq
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var propertyNames = properties.Select(p => p.Name);

            var table = new DataTable();

            table.Columns.AddRange(
                propertyNames.Select(p => new DataColumn(p)).ToArray()
            );

            foreach (var item in enumerable)
            {
                table.Rows.Add(properties.Select(p => p.GetValue(item)).ToArray());
            }

            return table;
        }
    }
}