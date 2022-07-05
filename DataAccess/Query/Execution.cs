using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace DataAccess.Query
{
    class Execution
    {
        readonly string connectionString;
        readonly SqlConnection cn;

        public Execution(string connectionString)
        {
            this.connectionString = connectionString;
            this.cn = new SqlConnection(this.connectionString);
        }

        public DataSet Execute_DataSet(string Query, SqlParameter[] Parameters)
        {
            SqlCommand cmd = new SqlCommand(Query, this.cn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = Query;
            cmd.CommandTimeout = 3000;

            if (Parameters != null)
            {
                foreach (SqlParameter parameter in Parameters)
                {
                    parameter.DbType = parameter.DbType;
                    parameter.ParameterName = parameter.ParameterName;
                    parameter.Value = parameter.Value;
                    cmd.Parameters.Add(parameter);
                }
            }

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }

        long Execute_NonQuery(string Query, SqlParameter[] Parameters, string ReturnID)
        {
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(Query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                if (Parameters != null)
                {
                    foreach (SqlParameter parameter in Parameters)
                    {
                        parameter.SqlDbType = parameter.SqlDbType;
                        parameter.ParameterName = parameter.ParameterName;
                        parameter.Value = parameter.Value;
                        cmd.Parameters.Add(parameter);
                    }
                }
                cmd.Parameters.Add(@ReturnID, SqlDbType.Int).Direction = ParameterDirection.Output;
                int a = cmd.ExecuteNonQuery();
                string id = cmd.Parameters[@ReturnID].Value.ToString();
                return Convert.ToInt64(id);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
            }
        }

        public object Execute_Scaler(string Query, SqlParameter[] Parameters)
        {
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(Query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                if (Parameters != null)
                {
                    foreach (SqlParameter parameter in Parameters)
                    {
                        parameter.SqlDbType = parameter.SqlDbType;
                        parameter.ParameterName = parameter.ParameterName;
                        parameter.Value = parameter.Value;
                        cmd.Parameters.Add(parameter);
                    }
                }
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
            }
        }

    }
}
