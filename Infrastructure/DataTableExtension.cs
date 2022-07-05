using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace eauction.Infrastructure
{
    public static class DataTableExtension
    {
        private static readonly IDictionary<Type, ICollection<PropertyInfo>> _Properties =
            new Dictionary<Type, ICollection<PropertyInfo>>();

        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static IEnumerable<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                var objType = typeof(T);
                ICollection<PropertyInfo> properties;

                lock (_Properties)
                {
                    if (!_Properties.TryGetValue(objType, out properties))
                    {
                        properties = objType.GetProperties().Where(property => property.CanWrite).ToList();
                        _Properties.Add(objType, properties);
                    }
                }

                var list = new List<T>(table.Rows.Count);

                //foreach (var row in table.AsEnumerable().Skip(1))
                foreach (var row in table.AsEnumerable())
                {
                    var obj = new T();

                    foreach (var prop in properties)
                    {
                        //try
                        //{
                        //    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        //    var safeValue = row[prop.Name] == null ? null : Convert.ChangeType(row[prop.Name], propType);

                        //    prop.SetValue(obj, safeValue, null);
                        //}
                        //catch
                        //{
                        //    // ignored
                        //}

                        if (table.Columns.Contains(prop.Name) == false)
                            prop.SetValue(obj, null, null);
                        else
                            if (row[prop.Name] == DBNull.Value)
                                prop.SetValue(obj, null, null);
                            else
                                prop.SetValue(obj, row[prop.Name], null);
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch(Exception ex)
            {
                return Enumerable.Empty<T>();
            }
        }
    }
}
