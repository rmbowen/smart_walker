using SmartWalkerApplication.Commands;
using SmartWalkerApplication.Commands.COMConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartWalkerApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate
            {
                COMConnection port = COMConnection.Instance;
                port.sendString("N");
                port.sendString("51010");
            };
            CommandParser parser = new CommandParser();
            parser.start();
        }
    }
}
