using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartWalkerApplication.Commands;

namespace SmartWalkerApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandParser parser = new CommandParser();
            parser.start();
        }
    }
}
