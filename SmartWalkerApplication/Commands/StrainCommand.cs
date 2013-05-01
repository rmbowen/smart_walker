using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class StrainCommand
    {

        private COMConnection.COMConnection port;

        public StrainCommand()
        {
            port = COMConnection.COMConnection.Instance;
        }


        public void start()
        {
            getStrain1();
            getStrain2();
            getStrain3();
            getStrain4();
        }

        private int getStrain1()
        {
            port.sendString("1");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int strain = int.Parse(port.readLineString());
            Console.WriteLine("First Strain Value: " + strain);
            return strain;
        }

        private int getStrain2()
        {
            port.sendString("2");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int strain = int.Parse(port.readLineString());
            Console.WriteLine("Second Strain Value: " + strain);
            return strain;
        }

        private int getStrain3()
        {
            port.sendString("3");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int strain = int.Parse(port.readLineString());
            Console.WriteLine("Third Strain Value: " + strain);
            return strain;
        }

        private int getStrain4()
        {
            port.sendString("4");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int strain = int.Parse(port.readLineString());
            Console.WriteLine("Fourth Strain Value: " + strain);
            return strain;
        }

    }
}
