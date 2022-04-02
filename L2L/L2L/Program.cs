using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2L
{
    internal class Program
    {
        static void Main(string[] args) {

            StartValidationService startValidationService = new StartValidationService(args);
            string result = startValidationService.Validate();
            if (!string.IsNullOrWhiteSpace(result)) {
                Console.WriteLine(result);
                return;
            }

            string fileHandle = args[0];
            BackupService backup = new BackupService(fileHandle);
            backup.Create();




        }
    }
}
