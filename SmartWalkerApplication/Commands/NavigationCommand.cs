using SmartWalker;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Microsoft.Kinect;
using System.Diagnostics; 

namespace SmartWalkerApplication.Commands
{
    class NavigationCommand
    {
        static SmartWalkerKinect walkerKinect;
        private bool endProgram = false;
        static SerialPort port;

        public NavigationCommand()
        {

        }


        public void start()
        {
            // Start the Kinect Program Piece
            startKinect();

            getIMUData();

            // Send data to IMU?
            Console.WriteLine("Send:");

            for (; ; )
            {
                Console.WriteLine(" ");
                Console.WriteLine("> ");
                port.WriteLine(Console.ReadLine());
            }
        }

        private void startKinect()
        {
            walkerKinect = new SmartWalkerKinect();

            walkerKinect.startKinect();


            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            // Set the Interval to 10 seconds.
            aTimer.Interval = 10000;
            aTimer.Enabled = true;
            while (endProgram) { }
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            walkerKinect.printMap();
            walkerKinect.stopKinect();
            endProgram = true;
        }

        private void getIMUData()
        {
            foreach (string p in SerialPort.GetPortNames())
            {
                Console.WriteLine(p);
            }

            int baud;
            string name;

            Console.Write("Port Name:");

            name = Console.ReadLine();
            Console.WriteLine(" ");
            Console.WriteLine("Baud rate:");
            baud = GetBaudRate();

            Console.WriteLine("Beging Serial...");

            BeginSerial(baud, name);
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();

            Console.WriteLine("Serial Started.");
            Console.WriteLine(" ");
            Console.WriteLine("Ctrl+C to exit program");

        }

        // asdfasdf
        static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            for (int i = 0; i < (10000 * port.BytesToRead) / port.BaudRate; i++)
                ;       //Delay a bit for the serial to catch up
            Console.Write(port.ReadExisting());
            //Console.WriteLine("");
            //Console.WriteLine("> ");
        }


        // Function to get the baud rate
        static int GetBaudRate()
        {
            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid integer.  Please try again:");
                return GetBaudRate();
            }
        }

        // Begin serial
        static void BeginSerial(int baud, string name)
        {
            port = new SerialPort(name, baud);
        }
    }

}
