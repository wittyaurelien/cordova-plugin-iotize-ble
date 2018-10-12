using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTizeBLE.Utility
{
    internal class Log
    {
        private static String textLog = "";

        private static Windows.Storage.StorageFile LogFile = null;

        public async static void CreateLog()
        {
            return;
            // Create sample file; replace if exists.
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            Log.LogFile =
                await storageFolder.CreateFileAsync("BLElog.txt",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Windows.Storage.FileIO.WriteTextAsync(Log.LogFile, "Execution Log:\n");

        }

        public async static void RemoveLog()
        {
            if (LogFile != null)
            {
                await LogFile.DeleteAsync();
            }
        }

        public static   void WriteLine(String line , params object [] list)
        {
            return;
            if (Log.LogFile == null)
            {
                Log.CreateLog();
            }

            Log.textLog += String.Format(line, list); 
            Log.textLog += "\n";

            if (Log.LogFile != null)
                Windows.Storage.FileIO.AppendTextAsync( Log.LogFile , line  + "\n");
        }
        

    }
}
