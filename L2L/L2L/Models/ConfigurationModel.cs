using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2L.Models
{

    /// <summary>
    /// Project specific configuration parameters
    /// </summary>
    internal class ConfigurationModel
    {

        /// <summary>
        /// Any namespace conversions you may need if you currently have both the linq2sql
        /// and LinqConnect namespaces active within the same project.
        /// </summary>
        public Dictionary<string, string> NamespaceUpdates { get; set; } 
        /// <summary>
        /// Name of the linq2sql datacontext we are moving away from
        /// </summary>
        public string SourceDataContextName { get; set; }
        /// <summary>
        /// Name of the target LinqConnect datacontext we are moving towards to
        /// </summary>
        public string TargetDataContextName { get; set; }

        /// <summary>
        /// We will query the database later on for field names, so we need a valid link
        /// to a copy of the database we are working against.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Variable used to access datacontext objects, which will be followed by tablenames
        /// </summary>
        public string TableDetectionContextAccess { get; set; }

        public ConfigurationModel() {
            SourceDataContextName = "AsterixDataContext";
            TargetDataContextName = "LcAsterixDataContext";
            NamespaceUpdates = new Dictionary<string, string>();
            NamespaceUpdates.Add("using Asterix.Framework.Data.Domain;", "using Asterix.Framework.Data.LcDomain;");
            TableDetectionContextAccess = "dc.";
            ConnectionString = System.IO.File.ReadAllText(@"C:\Users\bno.CORP\OneDrive\Git\Linq2Linq\connectionstring.secret");
        }

        
    }
}
