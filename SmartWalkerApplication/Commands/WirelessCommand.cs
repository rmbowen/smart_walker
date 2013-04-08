using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class WirelessCommand
    {
        public WirelessCommand()
        {

        }


        public void sendEmail(string strFrom
                             , string strTo
                             , string strSubject
                             , string strBody)
        {
            var fromAddress = new MailAddress(strFrom);
            var toAddress = new MailAddress(strTo);
            const string fromPassword = "Z2t6jv6m8r4.";
            const string subject = "Subject";
            const string body = "Body";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
