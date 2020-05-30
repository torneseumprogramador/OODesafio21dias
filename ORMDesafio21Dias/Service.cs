using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ORMDesafio21Dias
{
    public sealed class Service
    {
        private CType cType;

        public Service(CType _cType)
        {
            this.cType = _cType;
        }

        public void Save()
        {
            using (SqlConnection conn = new SqlConnection(this.cType.ConnectionString))
            {
                List<string> cols = new List<string>();
                List<object> values = new List<object>();

                foreach (var p in this.cType.GetType().GetProperties())
                {
                    if (p.GetValue(this.cType) == null) continue;

                    TableAttribute[] propertyAttributes = (TableAttribute[])p.GetCustomAttributes(typeof(TableAttribute), false);
                    if (propertyAttributes != null && propertyAttributes.Length > 0)
                    {
                        if (!propertyAttributes[0].IsNotOnDataBase && string.IsNullOrEmpty(propertyAttributes[0].PrimaryKey))
                        {
                            cols.Add(p.Name);
                            values.Add(p.GetValue(this.cType));
                        }
                    }
                    else
                    {
                        cols.Add(p.Name);
                        values.Add(p.GetValue(this.cType));
                    }
                }


                string table = getTableName(this.cType);


                string sql = string.Empty;
                if (this.cType.Id == 0)
                {
                    sql = $"insert into {table} (";
                    sql += string.Join(',', cols);
                    sql += ") values ( ";
                    sql += "@" + string.Join(", @", cols);
                    sql += ")";
                }
                else
                {
                    sql = $"update {table} set ";

                    var colsUpdate = new List<string>();
                    foreach (string col in cols)
                    {
                        colsUpdate.Add($"{col}=@${col}");
                    }
                    sql += string.Join(',', colsUpdate);

                    sql += $"where {this.getPkName()} = {this.cType.Id}";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                for (var i = 0; i < cols.Count; i++)
                {
                    var value = values[i];
                    var col = cols[i];

                    cmd.Parameters.Add($"@{col}", GetDbType(value));
                    cmd.Parameters[$"@{col}"].Value = value;
                }

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private SqlDbType GetDbType(object value)
        {
            var result = SqlDbType.VarChar;

            try
            {
                Type type = value.GetType();

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        result = SqlDbType.Variant;
                        break;
                    case TypeCode.Boolean:
                        result = SqlDbType.Bit;
                        break;
                    case TypeCode.Char:
                        result = SqlDbType.NChar;
                        break;
                    case TypeCode.SByte:
                        result = SqlDbType.SmallInt;
                        break;
                    case TypeCode.Byte:
                        result = SqlDbType.TinyInt;
                        break;
                    case TypeCode.Int16:
                        result = SqlDbType.SmallInt;
                        break;
                    case TypeCode.UInt16:
                        result = SqlDbType.Int;
                        break;
                    case TypeCode.Int32:
                        result = SqlDbType.Int;
                        break;
                    case TypeCode.UInt32:
                        result = SqlDbType.BigInt;
                        break;
                    case TypeCode.Int64:
                        result = SqlDbType.BigInt;
                        break;
                    case TypeCode.UInt64:
                        result = SqlDbType.Decimal;
                        break;
                    case TypeCode.Single:
                        result = SqlDbType.Real;
                        break;
                    case TypeCode.Double:
                        result = SqlDbType.Float;
                        break;
                    case TypeCode.Decimal:
                        result = SqlDbType.Money;
                        break;
                    case TypeCode.DateTime:
                        result = SqlDbType.DateTime;
                        break;
                    case TypeCode.String:
                        result = SqlDbType.VarChar;
                        break;
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }

        public void Destroy()
        {
            using (SqlConnection conn = new SqlConnection(this.cType.ConnectionString))
            {
                string sql = $"delete from {getTableName(this.cType)} where {this.getPkName()} = {this.cType.Id}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void Get()
        {
            using (SqlConnection conn = new SqlConnection(this.cType.ConnectionString))
            {
                string sql = $"select * from {getTableName(this.cType)} where {this.getPkName()} = {this.cType.Id}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            fill(this.cType, dr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private string  getPkName()
        {
            return this.cType.GetType().GetProperty("Id").GetCustomAttribute<TableAttribute>().PrimaryKey;
        }

        public List<CType> All()
        {
            string sql;

            var list = new List<CType>();
            using (SqlConnection conn = new SqlConnection(this.cType.ConnectionString))
            {
                sql = $"select * from {getTableName(this.cType)}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var instance = (CType)Activator.CreateInstance(this.cType.GetType());
                            fill(instance, dr);
                            list.Add(instance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return list;
            }
        }

        public static List<T> All<T>()
        {
            List<T> result = new List<T>();

            T item = (T)Activator.CreateInstance(typeof(T));
            var cnnString = item.GetType().GetProperty("ConnectionString").GetValue(item).ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                string sql;
                sql = $"select * from {getTableName(item)}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            T instance = (T)Activator.CreateInstance(typeof(T));
                            fill(instance, dr);
                            result.Add(instance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        private static void fill<T>(T obj, SqlDataReader dr)
        {
            foreach (var p in obj.GetType().GetProperties())
            {
                TableAttribute propertyAttribute = p.GetCustomAttribute<TableAttribute>();
                if (propertyAttribute != null)
                {
                    if (!propertyAttribute.IsNotOnDataBase)
                    {
                        if (!string.IsNullOrEmpty(propertyAttribute.PrimaryKey))
                        {
                            if (dr[propertyAttribute.PrimaryKey] != DBNull.Value)
                                p.SetValue(obj, dr[propertyAttribute.PrimaryKey]);
                        }
                        else if (dr[p.Name] != DBNull.Value)
                        {
                            p.SetValue(obj, dr[p.Name]);
                        }
                    }
                }
                else if (dr[p.Name] != DBNull.Value)
                {
                    p.SetValue(obj, dr[p.Name]);
                }
            }
        }

        private static string getTableName<T>(T item)
        {
            var table = $"{item.GetType().Name.ToLower()}s";

            TableAttribute tableAttribute = item.GetType().GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                table = tableAttribute.Name;
            }
            return table;
        }

        public static void DropTable<T>()
        {
            T item = (T)Activator.CreateInstance(typeof(T));
            var cnnString = item.GetType().GetProperty("ConnectionString").GetValue(item).ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                string sql;

                sql = $"DROP TABLE {getTableName(item)}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void CreateTable<T>()
        {
            T item = (T)Activator.CreateInstance(typeof(T));
            var cnnString = item.GetType().GetProperty("ConnectionString").GetValue(item).ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                string sql;

                sql = $"CREATE TABLE {getTableName(item)}" +
                    $"(" +
                    $"id int IDENTITY(1, 1),";
                sql += fildsCreateTable(item);
                sql += $")";

                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static string fildsCreateTable<T>(T item)
        {
            List<string> fields = new List<string>();

            foreach (var p in item.GetType().GetProperties())
            {
                TableAttribute propertyAttribute = p.GetCustomAttribute<TableAttribute>();
                if (propertyAttribute != null)
                {
                    if (!propertyAttribute.IsNotOnDataBase && string.IsNullOrEmpty(propertyAttribute.PrimaryKey))
                    {
                        fields.Add($"{p.Name} {getSqlTypeOfProperty(p)}");
                    }
                }
                else
                {
                    fields.Add($"{p.Name} varchar(20)");
                }
            }

            return string.Join(',', fields);
        }

        private static string getSqlTypeOfProperty(PropertyInfo p)
        {
            string type;
            switch (Type.GetTypeCode(p.PropertyType))
            {
                case TypeCode.String:
                    type = "varchar(255)";
                    break;
                case TypeCode.Boolean:
                    type = "tinyint";
                    break;
                case TypeCode.Int32:
                    type = "int";
                    break;
                case TypeCode.Double:
                    type = "DECIMAL (10, 2)";
                    break;
                case TypeCode.DateTime:
                    type = "DateTime";
                    break;
                default:
                    type = "varchar(255)";
                    break;
            }

            return type;
        }
    }
}
