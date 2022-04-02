

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace L2L
{


    public sealed class Helpers
    {
        /// <summary>
        ///     Returns a list of columns in a specific table
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal static List<string> GetColumnsInTable(string database, string tablename) {
            var retVal = new List<string>();
            using (SqlConnection con = new SqlConnection(database)) {
                con.Open();
                using (SqlCommand cmd = new SqlCommand($"exec sp_columns [{tablename}]", con)) {
                    SqlDataReader oReader = cmd.ExecuteReader();
                    while (oReader.Read()) {
                        string sColname = oReader["column_name"].ToString();
                        retVal.Add(sColname);
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Normalize table and field names to the format used by LinqConnect
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string Normalize(string value) {
            value = Capitalize(value);
            if (value.Contains("_")) {
                string[] args = value.Split(Convert.ToChar("_"));
                value = string.Empty;
                foreach (string arg in args) {
                    value += Capitalize(arg);
                }
            }
            return value;
        }

        internal static string Capitalize(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                throw new ArgumentException("value cannot be empty or null");
            }
            if (value.Length == 1) {
                return value.ToUpper();
            }
            return value.Substring(0, 1).ToUpper() + value.Substring(1);
        }

    }
}