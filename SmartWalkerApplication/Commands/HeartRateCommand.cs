using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartWalkerApplication.Commands
{
    class HeartRateCommand
    {

        public string start()
        {
            
            // Taking video .exe TAKES FOREVER to start running

            /*
            // Process to call the .exe to take an .avi video
            Process TakeVideo = new Process 
            {
                StartInfo = 
                {
                    FileName = "C:\\HR\\TakeVideo.exe",
                    WorkingDirectory = "C:\\HR",
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            TakeVideo.Start();
            TakeVideo.WaitForExit();
            */

            // Process to call the HR algorithm .exe
            Process HR = new Process
            {
                StartInfo =
                {
                    FileName = "C:\\Users\\Public\\Development\\smart_walker\\Heart Rate\\HRAlgorithm.exe",
                    //C:\Users\Public\Development\smart_walker\Heart Rate
                    WorkingDirectory = "C:\\Users\\Public\\Development\\smart_walker\\Heart Rate",
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            HR.Start();

            // Close the three debugging figures
            System.Threading.Thread.Sleep(16000);
            SendKeys.SendWait("%{f4}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("%{f4}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("%{f4}");

            HR.WaitForExit();


            // Read the HR from the file
            var reader = new StreamReader("C:\\Users\\Public\\Development\\smart_walker\\Heart Rate\\\\lastpulse.txt");

            string HeartRate = reader.ReadLine();

            Console.WriteLine(HeartRate);

            return (HeartRate);
        }

    }
}
