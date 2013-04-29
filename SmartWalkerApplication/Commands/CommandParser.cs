using SmartWalkerApplication.Commands.HUB;
using SmartWalkerApplication.Commands.Vitals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class CommandParser
    {

        public CommandParser()
        {
        }

        public void start()
        {
            while (true)
            {
                Console.WriteLine("Enter a command: ");
                string input = Console.ReadLine();
                Console.WriteLine();
                string[] words = input.Split(' ');
                bool invalidCommand = true;

                if (words[0].Equals("Mic"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Mic Array Command Entered");
                        invalidCommand = false;
                    }
                }
                else if(words[0].Equals("Navigation"))
                {
                    if (words.Length == 2)
                    {
                        Console.WriteLine("Navigation Command Entered");
                        invalidCommand = false;

                        NavigationCommand nc = new NavigationCommand(int.Parse(words[1]));
                        nc.start();
                        //nc.startKinect();
                        //SmartWalkerKinect
                        //nc.getIMUData();

                    }
                }
                else if (words[0].Equals("Force"))
                {
                    if (words.Length == 2)
                    {
                        Console.WriteLine("Force Command Entered");
                        invalidCommand = false;

                        ForceCommand fc = new ForceCommand();
                        fc.start();
                    }
                }
                else if (words[0].Equals("Heart"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Heart Rate Command Entered");
                        invalidCommand = false;

                        HeartRateCommand hrc = new HeartRateCommand();
                        hrc.start();
                    }
                }
                else if (words[0].Equals("Thermal"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Thermal Command Entered");
                        invalidCommand = false;

                        ThermalCommand tc = new ThermalCommand();
                        string temperature = tc.start();
                        Console.WriteLine(temperature);
                        Thermal thermal = new Thermal(temperature);
                        XMLStore xml = XMLStore.Instance;
                        xml.thermal.AddLast(thermal);
                    }
                }
                else if (words[0].Equals("Wireless"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Wireless Command Entered");
                        invalidCommand = false;

                        WirelessCommand wc = new WirelessCommand();
                        wc.sendEmail("thomasdemeo@gmail.com", "thomasdemeo@gmail.com", "Smart Walker Data", null);
                    }
                }
                if (invalidCommand) {
                    Console.WriteLine(input + " is not a valid command");
                    Console.WriteLine("Valid Commands Include:");
                    Console.WriteLine("Mic (read microphone array)");
                    Console.WriteLine("Navigation [degree] (start navigation system)");
                    Console.WriteLine("Force [Left/Right] (read force sensors)");
                    Console.WriteLine();

                }
            }
        }
    }

    
}
