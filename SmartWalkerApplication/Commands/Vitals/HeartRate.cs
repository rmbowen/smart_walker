using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands.Vitals
{
    class HeartRate
    {
        private double hr;

        private string startTag = "<heart_rate>";
        private string endTag = "</heart_rate>";

        public HeartRate(string hr)
        {
            if (hr != null)
            {
                this.hr = Double.Parse(hr);
            }
        }

        public string toXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(startTag);

            sb.Append("<value>" + hr + "</value>");

            sb.Append(endTag);

            return sb.ToString();
        }
    }
}
