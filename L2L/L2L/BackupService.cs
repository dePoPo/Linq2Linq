
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2L
{

    /// <summary>
    /// Craetes a backup copy of the file to be processed in the working directory
    /// and another copy in a random temp file to be used as a source reader when we
    /// rewrite the targetfile.
    /// </summary>
    internal class BackupService
    {
        private string _fileHandle;

        public BackupService(string fileHandle) {
            _fileHandle = fileHandle;
        }

        public string Create() {
            string target = $"{_fileHandle}.bak";
            File.Copy(_fileHandle, target, true);

            string temptarget = Path.GetTempFileName();
            File.Copy(_fileHandle, temptarget, true);
            return temptarget;
        }
    }
}
