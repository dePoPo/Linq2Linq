//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\Linq2Linq\L2L\L2L"
//  File="BackupService.cs" 
//  Modified="zaterdag 2 april 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2L
{
    internal class BackupService
    {
        private string _fileHandle;

        public BackupService(string fileHandle) {
            _fileHandle = fileHandle;
        }

        public void Create() {
            string target = $"{_fileHandle}.bak";
            File.Copy(_fileHandle, target, true);
        }
    }
}
