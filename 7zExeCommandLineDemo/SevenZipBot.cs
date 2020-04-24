
using _7zExeCommandLineDemo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zExeCommandLineDemo
{

    internal sealed class SevenZipBot
    {
        private const int ProcessTimeOut = 10000;
        private const int ProcessCheckListTimeOut = 300;
        private const string SEVENZIPEXEFILEPATH = "7-Zip/7z.exe";

        public static SevenZipBot Instance = new SevenZipBot();
        private List<FileModel> fileModels = new List<FileModel>();
        private bool startRead = false;
        private bool isrunning = false;

        #region innerClass
        public enum DuplicateOperate
        {
            Overwrite,  // -aoa means to directly overwrite the existing file without prompting. Similar:
            Skip,       // -aos skip existing files will not be overwritten
            Rename,     // -aou If a file with the same file name already exists, it will automatically rename the released file
            RenameOld   // -aot If a file with the same file name already exists, it will automatically rename the existing file
        }
        public class FeedBack<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
        public class FileModel
        {
            public DateTime FileDateTime { get; set; }

            public string Attr { get; set; }
            public long Size { get; set; }
            public long Compressed { get; set; }

            public string InnerFullPath { get; set; }

            public FileModel(string fromCommandLine)
            {
                var prop = fromCommandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                FileDateTime = DateTime.Parse($"{prop[0]} {prop[1]}");
                Attr = prop[2];
                Size = long.Parse(prop[3]);
                Compressed = long.Parse(prop[4]);
                InnerFullPath = prop[5];
            }
        }

        #endregion

        /// <summary>
        /// unzip files
        /// <param> -aoa means to directly overwrite the existing file without prompting. There are similar: </ param>
        /// <param> -aos will not overwrite existing files if skipped </ param>
        /// <param> -aou If a file with the same file name already exists, it will automatically rename the released file </ param>
        /// <param> -aot If a file with the same file name already exists, it will automatically rename the existing file </ param>
        /// </ summary>
        /// <param name = "zipFilePath"> source file </ param>
        /// <param name = "destPath"> target folder </ param>
        /// <param name = "Duplicateactor"> Operation options when overwriting </ param>
        /// <returns> </ returns>
        public bool DeCompress(string zipFilePath, string destPath, DuplicateOperate Duplicateactor = DuplicateOperate.Overwrite)
        {
            var operation = string.Empty;
            switch (Duplicateactor)
            {
                case DuplicateOperate.Overwrite:
                    operation = " -aoa";
                    break;
                case DuplicateOperate.Skip:
                    operation = " -aos";
                    break;
                case DuplicateOperate.Rename:
                    operation = " -aou";
                    break;
                case DuplicateOperate.RenameOld:
                    operation = " -aot";
                    break;
                default:
                    break;
            }

            Log.Debug($"DeCompress Start zipFilePath[{zipFilePath}]");
            Process process = new Process();
            bool succeeded = false;
            System.DateTime startTime = System.DateTime.Now;
            process.StartInfo.FileName = SEVENZIPEXEFILEPATH;
            process.StartInfo.UseShellExecute = false; // Whether to use the operating system shell to start
            process.StartInfo.RedirectStandardInput = true; // Accept input information from the calling program
            process.StartInfo.RedirectStandardOutput = true; // The output information is obtained by the calling program
            process.StartInfo.RedirectStandardError = true; // Redirect standard error output
            process.StartInfo.CreateNoWindow = true; // Do not display the program window
            process.StartInfo.Arguments = string.Format(@"x ""{0}"" -o""{1}"" {2}", zipFilePath, destPath, operation);
            // "x -o" "D:\User Directory\Download\0723.zip" "-aoa"
            process.ErrorDataReceived += ErrorDataReceived;
            process.OutputDataReceived += OutputDataReceived;
            process.Start(); // Start the program
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            Log.Debug($"SevenZipBot Start");
            process.WaitForExit(ProcessTimeOut); // Wait for the program to finish and exit the process
            succeeded = (startTime.AddMilliseconds(ProcessTimeOut) > System.DateTime.Now);

            process.Close();
            Log.Debug($"SevenZipBot End Arguments:" + zipFilePath + ", succeeded:" + succeeded);

            return succeeded;
        }

        /// <summary>
        /// Get the file list of compressed files
        /// </ summary>
        /// <param name = "Arguments"> </ param>
        /// <returns> </ returns>
        public FeedBack<List<FileModel>> GetFileList(string zipFilePath)
        {
            Log.Debug($"SevenZipBot End Arguments:" + zipFilePath);
            var result = new FeedBack<List<FileModel>>();
            startRead = false;
            fileModels.Clear();
            Process process = new Process();
            bool succeeded = false;
            System.DateTime startTime = System.DateTime.Now;
            process.StartInfo.FileName = SEVENZIPEXEFILEPATH;
            process.StartInfo.UseShellExecute = false; // Whether to use the operating system shell to start
            process.StartInfo.RedirectStandardInput = true; // Accept input information from the calling program
            process.StartInfo.RedirectStandardOutput = true; // The output information is obtained by the calling program
            process.StartInfo.RedirectStandardError = true; // Redirect standard error output
            process.StartInfo.CreateNoWindow = true; // Do not display the program window
            //process.StartInfo.Arguments = @ "l" "D:\User Directory\Download\0723.zip" "";
            process.StartInfo.Arguments = string.Format(@"l ""{0}""", zipFilePath);
            process.ErrorDataReceived += ErrorDataReceived;
            process.OutputDataReceived += OutputDataReceived;
            process.Start();//starting program
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            Log.Debug($"SevenZipBot Start ");
            process.WaitForExit(ProcessTimeOut);//Wait for the program to finish executing and exit the process  
            succeeded = (startTime.AddMilliseconds(ProcessCheckListTimeOut) > System.DateTime.Now);
            isrunning = true;
            while (isrunning && startTime.AddMilliseconds(ProcessCheckListTimeOut) > System.DateTime.Now)
            {
                //If you want to get stuck here, otherwise the program will be executed too fast. If you can't get the list, if the number of lists is large, it will be a little less than 3 seconds.
                System.Threading.Thread.Sleep(300);
            }
            result.Success = succeeded;
            result.Data = new List<FileModel>(fileModels);
            process.Close();
            Log.Debug($"SevenZipBot End Arguments:" + zipFilePath + ", succeeded:" + succeeded);

            return result;
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.Debug($"pdf Operation Error Message： {e.Data}");
            isrunning = false;
            Process process = sender as Process;
            process.Close();
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.Debug($"pdf Action Command line output： {e.Data}");
            Console.WriteLine(e.Data);
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (startRead)
                {
                    if (e.Data.StartsWith("-------------------"))
                    {
                        startRead = false;
                        isrunning = false;
                        return;
                    }
                    fileModels.Add(new FileModel(e.Data));
                }

                if (e.Data.Contains("-------------------"))
                {
                    startRead = true;
                    Process process = sender as Process;
                    process.Close();
                }
            }
        }


        //Scanning the drive for archives:
        //1 file, 51964594 bytes(50 MiB)

        // Listing archive: D:\User Directory\Download\dotnet451.zip

        // ---
        // Path = D:\User Directory\Download\dotnet451.zip
        //Type = zip
        //Physical Size = 51964594

        //   Date Time    Attr Size   Compressed Name
        //------------------- ----- ------------ ------------  ------------------------
        //2014-01-21 16:15:00 .....        31529         4066  Source\wpf\WindowsBase.csproj
        //2014-01-21 16:15:00 .....        31529         4066  Source\wpf\WindowsBase.csproj
        //------------------- ----- ------------ ------------  ------------------------
        //2014-02-12 15:54:34          278324055     47665754  18556 files


    }
}
