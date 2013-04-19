using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class ForceCommand
    {

        private COMConnection.COMConnection port;

        public ForceCommand() 
        {
           port = COMConnection.COMConnection.Instance;
        }


        public void start()
        {
            getLeftForce();
            getRightForce();
        }

        private int getLeftForce()
        {
            port.sendString("L");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int force = int.Parse(port.readLineString());
            Console.WriteLine("Left Force Value: " + force);
            return force;
        }

        private int getRightForce()
        {
            port.sendString("R");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int force = int.Parse(port.readLineString());
            Console.WriteLine("Right Force Value: " + force);
            return force;
        }
    }
}
