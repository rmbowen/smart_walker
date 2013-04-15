using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands.COMConnection
{
    class COMConnection
    {
        private static COMConnection instance;

        static SerialPort port;
        private const string portName = @"COM8";
        private const int baudRate = 9600;

        private COMConnection() { }

        public static COMConnection Instance
         {
             get 
             {
                  if (instance == null)
                  {
                     instance = new COMConnection();
                     port = new SerialPort(portName, baudRate);
                     port.Open();
                     Console.WriteLine("Connection Started.");
                  }
                return instance;
            }
         }

        public void sendString(string send)
        {
            port.WriteLine(send);
        }

        public void closeConnection()
        {
            instance = null;
            port.Close();
        }

        public string readLineString()
        {
            return port.ReadExisting();
        }

    }
}
