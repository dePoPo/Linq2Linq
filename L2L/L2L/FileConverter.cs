using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using L2L.Models;

namespace L2L
{

    /// <summary>
    /// The fileconverter does the actual work of converting out Linq2Sql format code
    /// to DevArt LinqConnect format code.
    /// 
    /// The first pass does simple conversions such as the datacontext names, and gathers
    /// names from tables that are encountered. The second pass does the conversion for
    /// other table references such as constructors, and convert code that accesses the
    /// fieldnames.
    /// </summary>
    internal class FileConverter
    {
        private string _sourcefile;
        private string _targetfile;
        private ConfigurationModel _config;
        private List<string> _fieldNames;
        private List<string> _tableNames;

        public FileConverter(string sourcefile, string targetfile, ConfigurationModel model) {
            _sourcefile = sourcefile;
            _targetfile = targetfile;
            _config = model;
            _fieldNames = new List<string>();
            _tableNames = new List<string>();
        }

        public void Process() {
            // first pass, scanning for names and some basic updates
            var reader = new StreamReader(_sourcefile);
            var writer = new StreamWriter(_targetfile);
            while (!reader.EndOfStream) {
                string buffer = reader.ReadLine();
                buffer = UpdateDataContext(buffer);
                buffer = UpdateNameSpaces(buffer);
                
                // update query access, and build a list of used tables and fields
                buffer = UpdateQueryStart(buffer);

                // flush oputput
                writer.WriteLine(buffer);
            }
            reader.Close();
            writer.Close();

            // second pass, tables and fields
            File.Copy(_targetfile, _sourcefile, true);
            var reader2 = new StreamReader(_sourcefile);
            var writer2 = new StreamWriter(_targetfile);
            while (!reader2.EndOfStream) {
                string buffer = reader2.ReadLine();

                // now we are aware of tables, we can update them in constructors etc.
                buffer = UpdateTables(buffer);
                buffer = UpdateFields(buffer);

                writer2.WriteLine(buffer);
                Console.WriteLine(buffer);
            }
            reader2.Close();
            writer2.Close();
        }

        public string UpdateDataContext(string buffer) {
            string find = $"new {_config.SourceDataContextName}(";
            string replace = $"new {_config.TargetDataContextName}(";
            string value = buffer.Replace(find, replace);
            return value;
        }

        public string UpdateNameSpaces(string buffer) {
            string value = buffer;
            foreach (var nspace in _config.NamespaceUpdates) {
                value = value.Replace(nspace.Key, nspace.Value);
            }
            return value;
        }


        public string UpdateQueryStart(string buffer) {
            string value = buffer;

            if (!buffer.Contains(_config.TableDetectionContextAccess)) {
                return value;
            }

            // table name will start at the found location, and continue to the next .
            // like dc.Asterix_klanten_Contactpersonen.Single(x => x.Id == id); from which we need
            // Asterix_klanten_Contactpersonen
            int startindex = buffer.IndexOf(_config.TableDetectionContextAccess) + _config.TableDetectionContextAccess.Length;
            string remainder = value.Substring(startindex);
            int nextdot = remainder.IndexOf(".");
            if (nextdot == -1) {
                // this is something like .SubmitChanges()
                return value;
            }
            string tablename = remainder.Substring(0, nextdot);
            string newname = Normalize(tablename);
            Console.WriteLine($"UpdateQueryStart::table: {tablename} -> {newname}");
            value = buffer.Replace(tablename, newname);

            // now that we are aware of the table, we store it's name to update things that dont need
            // the context such as new some_thing(); during a late pass
            _tableNames.Add(tablename);

            // get the names of the fields in this table, so we can update getters and setters for
            // the data objects fields in a later pass
            var fieldnames = L2L.Helpers.SqlTools.GetColumnsInTable(_config.ConnectionString, tablename);
            foreach (string field in fieldnames) {
                _fieldNames.Add(field);
            }
            return value;
        }

        /// <summary>
        /// Second pass table names
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public string UpdateTables(string buffer) {
            string value = buffer;
            foreach (string tablename in _tableNames) {
                if (value.Contains(tablename)) {
                    string targetname = Normalize(tablename);
                    value = value.Replace(tablename, targetname);
                }
            }
            return value;
        }

        /// <summary>
        /// Second pass field names, capitalize sql field names to get to teh linq field
        /// names, and normalize() for the LinqConnect names
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public string UpdateFields(string buffer) {
            string value = buffer;
            foreach(string field in _fieldNames) {
                string find = $".{Capitalize(field)}";
                string replace = $".{Normalize(field)}";
                if (buffer.Contains(find)) {
                    value = value.Replace(find, replace);
                }
            }
            return value;
        }

        /// <summary>
        /// Normalize table and field names to the format used by LinqConnect
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string Normalize(string value) {
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

        private static string Capitalize(string value) {
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
