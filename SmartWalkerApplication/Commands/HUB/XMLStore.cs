using SmartWalkerApplication.Commands.Vitals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands.HUB
{
    class XMLStore
    {

        private string XMLHeader = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
        private string startTag = "<smartwalker>";
        private string endTag = "</smartwalker>";

        public LinkedList<Force> force;
        public LinkedList<HeartRate> heartRate;
        public LinkedList<Strain> strain;
        public LinkedList<Thermal> thermal;

        private static XMLStore instance;

        private XMLStore() {
            force = new LinkedList<Force>();
            heartRate = new LinkedList<HeartRate>();
            strain = new LinkedList<Strain>();
            thermal = new LinkedList<Thermal>();
        }

        public static XMLStore Instance
         {
             get 
             {
                  if (instance == null)
                  {
                     instance = new XMLStore();

                  }
                return instance;
            }
         }

        public string getLatestDataAsXML()
        {
            StringBuilder HUBXMLString = new StringBuilder();
            HUBXMLString.Append(XMLHeader);
            HUBXMLString.Append(startTag);

            // Print all object XML here
            foreach (Thermal objectItem in thermal)
            {
                HUBXMLString.Append(objectItem.toXML());
            }

            HUBXMLString.Append(endTag);

            return HUBXMLString.ToString();
        }


    }
}
