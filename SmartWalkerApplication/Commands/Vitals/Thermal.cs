using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands.Vitals
{
    class Thermal
    {
        private double temperature;

        private string startTag = "<thermal>";
        private string endTag = "</thermal>";

        public Thermal(string temp)
        {
            if (temp != null)
            {
                try
                {
                    this.temperature = Double.Parse(temp);
                }
                catch (Exception e)
                {
                    this.temperature = 0.0;
                }
            }
        }

        public string toXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(startTag);

            sb.Append("<value>" + temperature + "</value>");

            sb.Append(endTag);

            return sb.ToString();
        }
    }
}
