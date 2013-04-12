using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class NavigationCommand
    {

        static SerialPort port;

        public NavigationCommand()
        {

        }


        private void start()
        {

            getIMUData();

            Console.WriteLine("Send:");

            for (; ; )
            {
                Console.WriteLine(" ");
                Console.WriteLine("> ");
                port.WriteLine(Console.ReadLine());
            }
        }

        public void getIMUData()
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
