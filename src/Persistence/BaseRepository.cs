using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Persistence
{
    public abstract class BaseRepository
    {
        public static string basePath = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// Initiate DB Connection 
        /// </summary>
        /// <returns></returns>
        public string ConnectionBuilder()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", false).Build();
            var connectionString = configuration.GetSection("DefaultConnection").Value;
            return connectionString;
        }

        /// <summary>
        /// Execute a stored procedure that doesn't return data
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="parameterList"></param>
        public void Exec(string storedProcedure, SqlParameter[] parameterList)
        {
            using (var cn = new SqlConnection(ConnectionBuilder()))
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameterList != null)
                        cmd.Parameters.AddRange(parameterList);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Get a list of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="parameterList"></param>
        /// <param name="cacheKey"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected List<T> GetList<T>(string storedProcedure, SqlParameter[] parameterList)
        {
            var list = new List<T>();
            using (var cn = new SqlConnection(ConnectionBuilder()))
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure; ;

                    if (parameterList != null)
                        cmd.Parameters.AddRange(parameterList);

                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        list = DataReaderToList<T>(dr);
                    }

                    cmd.Parameters.Clear();
                }
            }

            return list;
        }

        /// <summary>
        /// Convert a DataReader into an object of class T - Might be optimized
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static List<T> DataReaderToList<T>(IDataReader dr)
        {
            var list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                if (typeof(T).IsValueType)
                    list.Add((T)Convert.ChangeType(dr[0], typeof(T)));
                else
                {
                    obj = Activator.CreateInstance<T>();
                    foreach (PropertyInfo prop in obj.GetType().GetProperties())
                    {
                        if (ColumnExists(dr, prop.Name) && !object.Equals(dr[prop.Name], DBNull.Value))
                        {
                            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            if (type.IsEnum)
                            {
                                var value = Activator.CreateInstance(type);
                                try
                                {
                                    value = Enum.Parse(type, dr[prop.Name].ToString());
                                    prop.SetValue(obj, value, null);
                                }
                                catch (Exception)
                                {
                                    prop.SetValue(obj, value, null);
                                }
                            }
                            else
                                prop.SetValue(obj, Convert.ChangeType(dr[prop.Name], type), null);
                        }
                    }
                    list.Add(obj);
                }
            }
            return list;
        }

        /// <summary>
        /// Get Columns
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private static bool ColumnExists(IDataReader dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
