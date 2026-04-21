using KolayCAR.Broker.Domain.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public interface IDBHelper
    {
        DataTable GetDataTableToProc(string procName, SqlParameter[] sqlParams);
        void WriteLog(BrokerLogModel brokerLogModel);
    }

    public class DBConstants
    {
        public static readonly string VEHICLE_CLASS_OPERATIONS_PROC_NAME = "SP_VEHICLECLASS_OPERATIONS";
        public static readonly string AGENCY_OPERATIONS_PROC_NAME = "SP_AGENCY_OPERATIONS";
    }

    public class ADODBHelper : IDBHelper
    {
        public string ConnectionString { get; set; }

        public ADODBHelper(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public SqlConnection GetSQLConnection() => new SqlConnection(ConnectionString);

        public static SqlParameter GetSqlParameter(string name, object value)
        {
            SqlParameter spm = new SqlParameter();
            spm.ParameterName = name;
            spm.Value = value;
            return spm;
        }

        public DataTable GetDataTableToProc(string procName, SqlParameter[] sqlParams)
        {
            DataTable dt = new DataTable();

            SqlConnection con = GetSQLConnection();
            using (con)
            {
                con.Open();
                using (SqlCommand com = new SqlCommand(procName, con))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    if (sqlParams != null)
                    {
                        foreach (SqlParameter pm in sqlParams)
                        {
                            com.Parameters.Add(pm);
                        }
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(com))
                    {
                        da.Fill(dt);
                    }
                }

                con.Close();
                con.Dispose();
            }

            return dt;
        }

        public void WriteLog(BrokerLogModel brokerLogModel)
        {
            if (brokerLogModel != null && !string.IsNullOrEmpty(brokerLogModel.Content))
            {
                var parameters = new List<SqlParameter>
                {
                    GetSqlParameter("OPERATIONID", AgencyOperationTypes.WriteLog),
                    GetSqlParameter("LOGKEY", brokerLogModel.LogKey),
                    GetSqlParameter("CONTENT", brokerLogModel.Content),
                    GetSqlParameter("LOGTYPE", (int)brokerLogModel.LogType),
                    GetSqlParameter("LOGTYPEKEY", brokerLogModel.LogType.ToString())
                };

                GetDataTableToProc(DBConstants.AGENCY_OPERATIONS_PROC_NAME, parameters.ToArray());
            }
        }
    }
}
