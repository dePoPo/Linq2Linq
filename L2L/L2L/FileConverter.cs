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

            }
            reader2.Close();
            writer2.Close();
        }

        public string UpdateDataContext(string buffer) {
            string value = buffer;
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements.Add($"new {_config.SourceDataContextName}(", $"new {_config.TargetDataContextName}(");
            replacements.Add($" {_config.SourceDataContextName} ", $" {_config.TargetDataContextName} ");
            foreach (var item in replacements) {
                value = value.Replace(item.Key, item.Value);
            }
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
            // like dc.Some_Table_Name.Single(x => x.Id == id); from which we need
            // Some_Table_Name
            int startindex = buffer.IndexOf(_config.TableDetectionContextAccess) + _config.TableDetectionContextAccess.Length;
            string remainder = value.Substring(startindex);
            int nextdot = remainder.IndexOf(".");
            if (nextdot == -1) {
                // this is something like .SubmitChanges()
                return value;
            }
            string tablename = remainder.Substring(0, nextdot);
            string newname = Helpers.Normalize(tablename);
            value = buffer.Replace(tablename, newname);

            // now that we are aware of the table, we store it's name to update things that dont need
            // the context such as new some_thing(); during a late pass
            if (!_tableNames.Contains(tablename)) {
                _tableNames.Add(tablename);
            }

            // get the names of the fields in this table, so we can update getters and setters for
            // the data objects fields in a later pass
            var fieldnames = Helpers.GetColumnsInTable(_config.ConnectionString, tablename);
            foreach (string field in fieldnames) {
                if (!_fieldNames.Contains(field)) {
                    _fieldNames.Add(field);
                }
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
                    string targetname = Helpers.Normalize(tablename);
                    value = value.Replace(tablename, targetname);
                }
                if (value.Contains(Helpers.Capitalize(tablename))) {
                    string targetname = Helpers.Normalize(tablename);
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
            foreach (string field in _fieldNames) {
                Dictionary<string, string> replacements = new Dictionary<string, string>();
                replacements.Add($".{Helpers.Capitalize(field)}", $".{Helpers.Normalize(field)}");
                replacements.Add($"{Helpers.Capitalize(field)} =", $"{Helpers.Normalize(field)} =");
                foreach (var item in replacements) {
                    string find = item.Key;
                    string replace = item.Value;
                    if (buffer.Contains(find)) {
                        value = value.Replace(find, replace);
                    }
                }
            }
            return value;
        }
    }
}
