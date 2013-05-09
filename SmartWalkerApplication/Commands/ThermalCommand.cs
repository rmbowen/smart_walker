using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Runtime.InteropServices;



namespace SmartWalkerApplication.Commands
{


    class ThermalCommand
    {
        [DllImport("coredll.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public string start()
        {
            // Start the Thermal Program
            
            try
            {
                System.Diagnostics.Process.Start("C:\\Program Files (x86)\\OMEGA\\OSXL-101_ApplicationSoft Ver2_12\\OSXL-101.exe");
               /* Process[] p = Process.GetProcessesByName("OSXL-101");

                // Activate the first application we find with this name
                if (p.Count() > 0)
                    SetForegroundWindow(p[0].MainWindowHandle);*/
           

            

            // "Tab" twice to get onto Apply button and press "Enter"
            // to start software

            System.Threading.Thread.Sleep(4000);

            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(4000);

             SendKeys.SendWait("%{TAB}");

            // "Tab" once to get to "RUN" button and press "Enter"
            // to begin running
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(2000);

            // Send program "ALT + D" and "ALT + G" to open function
            // menu and to begin Zone trending
            SendKeys.SendWait("%dg");
            System.Threading.Thread.Sleep(5000);

            // Send program "ALT + D" and "ALT + S" to open trend data
            // and to save results
            SendKeys.SendWait("%ds");
            System.Threading.Thread.Sleep(1000);

            // Press Enter + Left + Enter to properly save out file
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("{LEFT}");
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(2000);

            // Close the Zone Trend window as well as the OSXL-101 software
            SendKeys.SendWait("%{f4}");
            System.Threading.Thread.Sleep(500);

            SendKeys.SendWait("% ");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{UP}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{ENTER}");

            // Read from graphData.csv file 
            var reader = new StreamReader("C:\\Users\\tjd9961\\Desktop\\graphData.csv");

            string line;

            // Iterate through the header of the .csv
            for (int i = 0; i < 11; ++i)
            {
                line = reader.ReadLine();
            }

            // Read the line containing the zones average temperature
            line = reader.ReadLine();

            // Split the date and temp apart
            var parts = line.Split(',');
            
            // Scale the temperature down
            double temp;

            double.TryParse(parts[1],out temp);

            if(temp > 41.0) {
                temp *= 0.865614361;
            }

            string temperature = Convert.ToString(temp);
            // Return the temp in degress C
            return temperature;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
        }
    }
}
