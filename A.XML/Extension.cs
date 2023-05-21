using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace A.XML
{
    public static partial class Extension
    {
        public static List<T> ToList<T>(this DataTable dataTable)
        {
            List<T> list = new List<T>();
            var columnas = new List<string>();
            foreach (DataColumn col in dataTable.Columns) columnas.Add(col.ColumnName);

            foreach (DataRow row in dataTable.Rows)
            {
                T obj = Activator.CreateInstance<T>();

                foreach (PropertyInfo prop in (from x in obj.GetType().GetProperties()
                                               join col in columnas on x.Name equals col
                                               select x))
                {
                    if (!Equals(row[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, row[prop.Name]);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        public static DataTable ToDataTable(this IEnumerable data, Action<CustomDataTable> options)
        {
            var custom = new CustomDataTable();
            options?.Invoke(custom);
            return data.ToDataTable(custom);
        }
        public static DataTable ToDataTable(this IEnumerable data)
        {
            var custom = new CustomDataTable();
            return data.ToDataTable(custom);
        }
        private static DataTable ToDataTable(this IEnumerable data, CustomDataTable options)
        {
            var typeData = data.AsQueryable().ElementType;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeData);
            DataTable table = new DataTable(options.TableName);
            foreach (PropertyDescriptor prop in properties)
            {
                var propertyType = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                if (underlyingType != null) propertyType = underlyingType;
                table.Columns.Add(new DataColumn { ColumnName = prop.Name, DataType = propertyType });
            }

            var columna = table.Columns;
            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (DataColumn prop in columna)
                {
                    var valor = properties[prop.ColumnName].GetValue(item);
                    if (valor != null) row[prop.ColumnName] = valor;
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
    public class CustomDataTable
    {
        public string TableName { get; set; } = "TableName";
    }
}
