using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2L
{
    internal class StartValidationService
    {
        private string[] _args;

        public StartValidationService(string[] args) {
            _args = args;
        }

        public string Validate() {
            if(_args.Length == 0) {
                return "usage: l2l <filename>";
            }

            if(!File.Exists(_args[0])) {
                return "file not found";
            }

            if(Path.GetExtension(_args[0]) != "cs") {
                return "invalid file type";
            }

            return string.Empty;
        }
    }
}
