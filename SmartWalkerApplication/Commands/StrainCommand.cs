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
            getLeftStrain();
            getRightStrain();

        }

        public int getLeftStrain()
        {
            port.sendString("1");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int strain = int.Parse(port.readLineString());

            // simple weight equation for left gauge
            strain = (strain - 875) / (-20);
            
            return strain;
        }

        public int getRightStrain()
        {
            port.sendString("2");
            //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(200);

            int strain = int.Parse(port.readLineString());

            // simple weight equation for right gauge
            strain = (strain - 875) / (-48);

            return strain;
        }
    }
}
