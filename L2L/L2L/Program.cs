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

            // create backup and working copy
            string fileHandle = args[0];
            BackupService backup = new BackupService(fileHandle);
            string tempsourcefile = backup.Create();

            // process file
            var model = new L2L.Models.ConfigurationModel();
            FileConverter scannerService = new FileConverter(tempsourcefile, fileHandle, model);
            scannerService.Process();

            // cleanup tempsource
            File.Delete(tempsourcefile);



        }
    }
}
