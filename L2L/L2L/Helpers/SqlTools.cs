// -------------------------------------------------------------------------------------------
// 
//      A        RuntimeComponents, Asterix.Framework.Foundation, MsSql.cs
//     A A       Last modified: 20-01-2022, 12:08
//    A   A      Author: bob noordam (noordam@derooderoos.nl, hp.noordam@mabosanjer.nl)
//   AAAAAAA     Copyright (c) De Roode Roos Holding BV. All rights reserved.
//  A       A    Technical documentation: http://asterixtechniek.enzovoort.net/
// A         A   User documentation: http://asterixenduser.enzovoort.net/
// 
// -------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace L2L.Helpers
{
    /// <summary>
    ///     LibSql offers easy access to data from commands and stores procedures as well as functions tot analyze tables,
    ///     views
    ///     and indexes in SQL Server database
    /// </summary>
    public sealed class SqlTools
    {
        /// <summary>
        ///     Count the number of lines in the resulset based on the given Query and connection string
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <returns></returns>
        public static int CountQuery(SqlCommand oCmd, string sConString) {
            int nCount = 0;
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCmd.Connection = oCon;
                oCon.Open();
                using (oCmd) {
                    using (SqlDataReader oReader = oCmd.ExecuteReader()) {
                        if (oReader.HasRows) {
                            while (oReader.Read()) {
                                nCount = nCount + 1;
                            }
                        }
                    }
                }
            }
            return nCount;
        }

        /// <summary>
        ///     Execute an sql command that does not return any values or data
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <param name="commandTimeout">Command timeout, default 90 seconds if omitted</param>
        /// <remarks></remarks>
        public static void ExecuteCommand(SqlCommand oCmd, string sConString, int commandTimeout = 90) {
            oCmd.CommandTimeout = commandTimeout;
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.Connection = oCon;
                oCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Executes the query and returns the first column of the first row in the result set
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <returns></returns>
        public static object ExecuteCommandScalar(SqlCommand oCmd, string sConString) {
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.Connection = oCon;
                return oCmd.ExecuteScalar();
            }
        }

        /// <summary>
        ///     Executes a single or multi line SQL query stored in sQuery against the given database.
        ///     The query is scanned for GO commands, and each statement is send seperately to the sql
        ///     server. This method is not intended for use with statements that do actual data processing,
        ///     use the binary save alternatives for that instead.
        /// </summary>
        /// <param name="sQuery"></param>
        /// <param name="sConString"></param>
        public static void ExecuteQuery(string sQuery, string sConString) {
            sQuery = sQuery.Replace("GO", "|");
            char split = char.Parse("|");
            string[] sCommandlist = sQuery.Split(split);
            foreach (string sCommand in sCommandlist) {
                using (SqlConnection oCon = new SqlConnection(sConString)) {
                    oCon.Open();
                    using (SqlCommand oCmd = new SqlCommand(sCommand, oCon)) {
                        oCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        ///     Execute a stored procedure that does not return any values or data
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <remarks></remarks>
        public static void ExecuteStoredProcedure(SqlCommand oCmd, string sConString) {
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.Connection = oCon;
                oCmd.CommandType = CommandType.StoredProcedure;
                oCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Executes the stored procedure and returns the first column of the first row in the result set
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <returns></returns>
        public static object ExecuteStoredProcedureScalar(SqlCommand oCmd, string sConString) {
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.Connection = oCon;
                oCmd.CommandType = CommandType.StoredProcedure;
                return oCmd.ExecuteScalar();
            }
        }

        /// <summary>
        ///     Returns a list of columns in a specific table
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <param name="sTablename"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetColumnsInTable(string sConnectionstring, string sTablename) {
            var oReturn = new List<string>();
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                using (SqlCommand oCmd = new SqlCommand($"exec sp_columns [{sTablename}]", oCon)) {
                    SqlDataReader oReader = oCmd.ExecuteReader();
                    while (oReader.Read()) {
                        string sColname = oReader["column_name"]
                            .ToString();
                        oReturn.Add(sColname);
                    }
                }
            }
            return oReturn;
        }

        /// <summary>
        ///     returns the c# compatible data type for a specified column in the specified datatable. This functionality
        ///     is implemented as a supporting function for the CRUDGEN code generator, and is not intended for direct usage
        ///     as it does not cover the full range of field types.
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <param name="sTablename"></param>
        /// <param name="sColumnname"></param>
        /// <returns></returns>
        public static string GetCTypeForColumn(string sConnectionstring, string sTablename, string sColumnname) {
            string sqltype = GetTypeForColumn(sConnectionstring, sTablename, sColumnname);
            if (sqltype == "sysname" || sqltype == "sysname?"
                                     || sqltype == "char" || sqltype == "char?"
                                     || sqltype == "nvarchar" || sqltype == "nvarchar?"
                                     || sqltype == "varchar" || sqltype == "varchar?"
                                     || sqltype == "ntext" || sqltype == "ntext?"
                                     || sqltype == "timestamp" || sqltype == "timestamp?") {
                return "string";
            }
            switch (sqltype) {
                case "image":
                case "uniqueidentifier":
                case "varbinary":
                    return "byte[]";
                case "int":
                    return "int";
                case "int?":
                    return "int?";
                case "tinyint":
                    return "byte";
                case "tinyint?":
                    return "byte?";
                case "smallint":
                    return "short";
                case "smallint?":
                    return "short?";
                case "bigint":
                    return "long";
                case "bigint?":
                    return "long?";
                case "datetime":
                case "date":
                    return "DateTime";
                case "datetime?":
                    return "DateTime?";
                case "bit":
                    return "bool";
                case "numeric":
                case "decimal":
                case "money":
                    return "decimal";
                case "real":
                    return "real";
                case "float":
                    return "double";
                case "float?":
                    return "double?";
            }
            return "UNKNOWNTYPE";
        }

        /// <summary>
        ///     Returns a list of databases available on the target server\instance
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetDataBaseList(string sTargetInstance) {
            var sReturn = new List<string>();
            string sConString = $"Data Source={sTargetInstance};Initial Catalog={"master"};Persist Security Info=True;User ID=asterix;Password=zbroggbvgw";
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                using (SqlCommand oCmd = new SqlCommand("select name from sys.databases order by name", oCon)) {
                    using (SqlDataReader oReader = oCmd.ExecuteReader()) {
                        if (oReader.HasRows) {
                            while (oReader.Read()) {
                                sReturn.Add(oReader["name"]
                                    .ToString());
                            }
                        }
                    }
                }
            }
            return sReturn;
        }

        /// <summary>
        ///     Run the passed SQL Command against the database, and return any results as a datatable object
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DataTable GetDataTableFromCommand(SqlCommand oCmd, string sConString) {
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.Connection = oCon;
                using (SqlDataAdapter oDa = new SqlDataAdapter(oCmd)) {
                    using (DataSet oDs = new DataSet()) {
                        oDa.Fill(oDs);
                        return oDs.Tables[0]
                            .Copy();
                    }
                }
            }
        }

        /// <summary>
        ///     Run the passed SQL command as a stored procedure against the database, and return any results as a datatable object
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConstring"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DataTable GetDataTableFromStoredProcedure(SqlCommand oCmd, string sConstring) {
            DataTable oTable = new DataTable();
            using (SqlConnection oCon = new SqlConnection(sConstring)) {
                oCon.Open();
                using (SqlDataAdapter oDa = new SqlDataAdapter()) {
                    oCmd.Connection = oCon;
                    oDa.SelectCommand = oCmd;
                    oDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    using (DataSet oDs = new DataSet()) {
                        oDa.Fill(oDs, "results");
                        oTable = oDs.Tables[0];
                    }
                }
            }
            return oTable.Copy();
        }

        /// <summary>
        ///     Returns the default database value for a specific field, in a specific table
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <param name="sTablename"></param>
        /// <param name="sColumnname"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetDefaultValueForColumn(string sConnectionstring, string sTablename, string sColumnname) {
            string sReturn = string.Empty;
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                using (SqlCommand oCmd = new SqlCommand($"exec sp_columns [{sTablename}]", oCon)) {
                    SqlDataReader oReader = oCmd.ExecuteReader();
                    while (oReader.Read()) {
                        string sColname = oReader["column_name"]
                            .ToString();
                        if (sColname.ToLower() == sColumnname.ToLower()) {
                            sReturn = oReader["column_def"]
                                .ToString();
                        }
                    }
                }
            }
            return sReturn;
        }

        /// <summary>
        ///     Get a list of non-unique indexes in the appointed database / table
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetIndexes(string database, string tablename) {
            var indexlist = new List<string>();
            using (SqlConnection connection = new SqlConnection(database)) {
                using (SqlCommand command = new SqlCommand($"exec sp_helpindex [{tablename}]", connection)) {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows) {
                        while (reader.Read()) {
                            string description = reader["index_description"].ToString();
                            string keys = reader["index_keys"].ToString();
                            if (description.IndexOf("unique") == -1 && keys.IndexOf(",") == -1) {
                                indexlist.Add(keys);
                            }
                        }
                    }
                }
            }
            return indexlist;
        }

        /// <summary>
        ///     Execute stored procedure and return the integer value of the @Return_Value
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetIntFromStoredProcedure(SqlCommand oCmd, string sConString) {
            int nReturn = 0;
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.CommandType = CommandType.StoredProcedure;
                oCmd.Connection = oCon;
                SqlParameter oReturn = new SqlParameter("@Return_Value", DbType.Int32);
                oReturn.Direction = ParameterDirection.ReturnValue;
                oCmd.Parameters.Add(oReturn);
                oCmd.ExecuteNonQuery();
                nReturn = (int) oCmd.Parameters["@Return_Value"]
                    .Value;
            }
            return nReturn;
        }

        /// <summary>
        ///     Achterhaal welke tabellen een koppeling hebben naar de opgegegeven tabel
        ///     dus bv. faktuurkop als vraag, levert factuur regels als antwoord
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<string> GetLinkingTables(string database, string tableName) {
            var result = new List<string>();
            SqlConnection con = new SqlConnection(database);
            SqlCommand cmd = new SqlCommand("select distinct schema_name(fk_tab.schema_id) + '.' + fk_tab.name as foreign_table, " +
                                            "schema_name(pk_tab.schema_id) + '.' + pk_tab.name as primary_table " +
                                            "from sys.foreign_keys fk inner join sys.tables fk_tab on fk_tab.object_id = fk.parent_object_id inner join sys.tables pk_tab " +
                                            "on pk_tab.object_id = fk.referenced_object_id where pk_tab.[name] = @tablename " +
                                            "order by schema_name(fk_tab.schema_id) + '.' + fk_tab.name, " +
                                            "schema_name(pk_tab.schema_id) + '.' + pk_tab.name");
            cmd.Parameters.AddWithValue("@tablename", tableName);
            cmd.Connection = con;
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                while (reader.Read()) {
                    string buffer = reader["foreign_table"].ToString();
                    result.Add(buffer);
                }
            }
            con.Close();
            return result;
        }

        /// <summary>
        ///     Bepaal de precisie van de opgegeven kolom in de sqlserver tabel
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <param name="sTablename"></param>
        /// <param name="sColumnname"></param>
        /// <returns></returns>
        public static string GetPrecisionForColumn(string sConnectionstring, string sTablename, string sColumnname) {
            string sReturn = string.Empty;
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                using (SqlCommand oCmd = new SqlCommand($"exec sp_columns [{sTablename}]", oCon)) {
                    SqlDataReader oReader = oCmd.ExecuteReader();
                    while (oReader.Read()) {
                        string sColname = oReader["column_name"]
                            .ToString();
                        if (sColname.ToLower() == sColumnname.ToLower()) {
                            sReturn = oReader["precision"]
                                .ToString();
                        }
                    }
                }
            }
            return sReturn;
        }

        /// <summary>
        ///     Execute stored provedure and return the string value of @Return_Value
        /// </summary>
        /// <param name="oCmd"></param>
        /// <param name="sConString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetStringFromStoredProcedure(SqlCommand oCmd, string sConString) {
            string sReturn;
            using (SqlConnection oCon = new SqlConnection(sConString)) {
                oCon.Open();
                oCmd.CommandType = CommandType.StoredProcedure;
                oCmd.Connection = oCon;
                SqlParameter oReturn = new SqlParameter("@Return_Value", DbType.String);
                oReturn.Direction = ParameterDirection.ReturnValue;
                oCmd.Parameters.Add(oReturn);
                oCmd.ExecuteNonQuery();
                sReturn = oCmd.Parameters["@Return_Value"]
                    .Value.ToString();
            }
            return sReturn;
        }

        /// <summary>
        ///     Get a list of tables in the appointed database
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetTables(string sConnectionstring) {
            var oReturn = new List<string>();
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                SqlCommand oCmd = new SqlCommand("SELECT * FROM sys.tables ORDER BY name", oCon);
                SqlDataReader oReader = oCmd.ExecuteReader();
                if (oReader.HasRows) {
                    while (oReader.Read()) {
                        oReturn.Add(oReader["name"]
                            .ToString());
                    }
                }
            }
            return oReturn;
        }

        /// <summary>
        ///     Get a list of tables and views in the appointed database
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetTablesAndViews(string sConnectionstring) {
            var oReturn = new List<string>();
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                SqlCommand oCmd = new SqlCommand("SELECT * FROM sys.tables ORDER BY name", oCon);
                SqlDataReader oReader = oCmd.ExecuteReader();
                if (oReader.HasRows) {
                    while (oReader.Read()) {
                        oReturn.Add(oReader["name"]
                            .ToString());
                    }
                }
            }
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                SqlCommand oCmd = new SqlCommand("SELECT * FROM sys.views ORDER BY name", oCon);
                SqlDataReader oReader = oCmd.ExecuteReader();
                if (oReader.HasRows) {
                    while (oReader.Read()) {
                        oReturn.Add(oReader["name"]
                            .ToString());
                    }
                }
            }
            return oReturn;
        }

        /// <summary>
        ///     Get the data type for a specific column in a specific table as readable text
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <param name="sTablename"></param>
        /// <param name="sColumnname"></param>
        /// <returns>SQL Server datat type for the column</returns>
        /// <remarks></remarks>
        public static string GetTypeForColumn(string sConnectionstring, string sTablename, string sColumnname) {
            string sReturn = string.Empty;
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                using (SqlCommand oCmd = new SqlCommand($"exec sp_columns [{sTablename}]", oCon)) {
                    SqlDataReader oReader = oCmd.ExecuteReader();
                    while (oReader.Read()) {
                        string columnname = oReader["column_name"].ToString();
                        int nullable = int.Parse(oReader["nullable"].ToString());
                        if (columnname.ToLower() == sColumnname.ToLower()) {
                            string retval = oReader["type_name"].ToString();
                            retval = retval.Replace("identity", "").Trim();
                            if (nullable == 1) {
                                retval += "?";
                            }
                            return retval;
                        }
                    }
                }
            }
            return sReturn;
        }

        /// <summary>
        ///     Get a list of unique indexes in the appointed database / table
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <param name="sTablename"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetUniqueIndexes(string sConnectionstring, string sTablename) {
            var oReturn = new List<string>();
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                using (SqlCommand oCmd = new SqlCommand($"exec sp_helpindex [{sTablename}]", oCon)) {
                    SqlDataReader oReader = oCmd.ExecuteReader();
                    if (oReader.HasRows) {
                        while (oReader.Read()) {
                            string sIndexname = oReader["index_name"]
                                .ToString();
                            string sDescription = oReader["index_description"]
                                .ToString();
                            string sKeys = oReader["index_keys"]
                                .ToString();
                            if (sDescription.IndexOf("unique") != -1) {
                                oReturn.Add(sKeys);
                            }
                        }
                    }
                }
            }
            return oReturn;
        }

        /// <summary>
        ///     Get a list of views in the appointed database
        /// </summary>
        /// <param name="sConnectionstring"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<string> GetViews(string sConnectionstring) {
            var oReturn = new List<string>();
            using (SqlConnection oCon = new SqlConnection(sConnectionstring)) {
                oCon.Open();
                SqlCommand oCmd = new SqlCommand("SELECT * FROM sys.views ORDER BY name", oCon);
                SqlDataReader oReader = oCmd.ExecuteReader();
                if (oReader.HasRows) {
                    while (oReader.Read()) {
                        oReturn.Add(oReader["name"]
                            .ToString());
                    }
                }
            }
            return oReturn;
        }
    }
}