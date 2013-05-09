using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands.Vitals
{
    class Force
    {
        public double leftForce;
        public double rightForce;

        public int leftStrain;
        public int rightStrain;

        private string startTag = "<force>";
        private string endTag = "</force>";

        public string toXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(startTag);
            sb.Append("<left_force>");
            sb.Append("<value>" + leftForce + "%" + "</value>");
            sb.Append("</left_force>");

            sb.Append("<right_force>");
            sb.Append("<value>" + rightForce + "%" + "</value>");
            sb.Append("</right_force>");

            sb.Append("<left_strain>");
            sb.Append("<value>" + leftStrain + "LB" + "</value>");
            sb.Append("</left_strain>");

            sb.Append("<right_strain>");
            sb.Append("<value>" + rightStrain + "LB" + "</value>");
            sb.Append("</right_strain>");

            sb.Append(endTag);

            return sb.ToString();
        }

    }
}
