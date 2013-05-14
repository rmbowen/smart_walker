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

            // Process to call the HR algorithm .exe
            Process HR = new Process
            {
                StartInfo =
                {
                    FileName = "C:\\Users\\Public\\Development\\smart_walker\\Heart Rate\\HRAlgV3.exe",
                    //C:\Users\Public\Development\smart_walker\Heart Rate
                    WorkingDirectory = "C:\\Users\\Public\\Development\\smart_walker\\Heart Rate",
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            HR.Start();

            System.Threading.Thread.Sleep(1000);
            ThermalCommand tc = new ThermalCommand();
            string temperature = tc.start();

            Console.WriteLine(temperature);

            HR.WaitForExit();


            // Read the HR from the file
            var reader = new StreamReader("C:\\Users\\Public\\Development\\smart_walker\\Heart Rate\\\\lastpulse.txt");

            string HeartRate = reader.ReadLine();

            Console.WriteLine(HeartRate);

            return (HeartRate);
        }

        public string startBoth()
        {
            // Process to call the HR algorithm .exe
            Process HR = new Process
            {
                StartInfo =
                {
                    FileName = "C:\\Users\\Public\\Development\\smart_walker\\Heart Rate\\HRAlgV2.exe",
                    //C:\Users\Public\Development\smart_walker\Heart Rate
                    WorkingDirectory = "C:\\Users\\Public\\Development\\smart_walker\\Heart Rate",
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            HR.Start();

            // Thermal command
            System.Threading.Thread.Sleep(1500);
            ThermalCommand tc = new ThermalCommand();
            string temperature = tc.start();

            HR.WaitForExit();


            // Read the HR from the file
            var reader = new StreamReader("C:\\Users\\Public\\Development\\smart_walker\\Heart Rate\\\\lastpulse.txt");

            string HeartRate = reader.ReadLine();

            return (HeartRate + " " +  temperature);
        }

    }
}
