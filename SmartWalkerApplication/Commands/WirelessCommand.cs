using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using SmartWalkerApplication.Commands.HUB;

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
           /* MailMessage mail = new MailMessage(new MailAddress(strFrom), new MailAddress(strTo));

            mail.Subject = strSubject;
            mail.Body = XMLStore.Instance.getLatestDataAsXML();

            SmtpClient smtpMail = new SmtpClient("smtp.gmail.com");
            smtpMail.Port = 465;
            smtpMail.EnableSsl = true;
            smtpMail.Credentials = new NetworkCredential("thomasdemeo@gmail.com", "Z2t6jv6m8r4.");
            // and then send the mail
            */
            var fromAddress = new MailAddress(strFrom);
            var toAddress = new MailAddress(strTo);
            const string fromPassword = "Z2t6jv6m8r4.";
            string subject = strSubject;
            string body = XMLStore.Instance.getLatestDataAsXML();

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
             
                try
                {
                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
           }
        }
    }
}
