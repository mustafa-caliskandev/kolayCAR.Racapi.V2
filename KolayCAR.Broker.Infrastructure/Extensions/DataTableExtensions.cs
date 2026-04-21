using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable GetSortedTable(this DataTable dt, string sort)
        {
            dt.DefaultView.Sort = sort;
            return dt.DefaultView.ToTable();
        }

        public static List<T> ConvertDataTable<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    try
                    {
                        if (pro.Name.ToLower(new System.Globalization.CultureInfo("en-EN")) == column.ColumnName.ToLower(new System.Globalization.CultureInfo("en-EN")))
                            pro.SetValue(obj, dr[column.ColumnName] == DBNull.Value ? null : dr[column.ColumnName], null);
                        else
                            continue;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            return obj;
        }
    }
}
