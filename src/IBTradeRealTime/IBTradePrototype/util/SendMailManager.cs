using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    class SendMailManager : ISendMailManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static volatile ISendMailManager instance;
        private static object syncRoot = new Object();

        private SendMailManager() { }

        public static ISendMailManager getManager()
        {
            if (instance == null) 
            {
                lock (syncRoot) 
                {
                    if (instance == null) 
                        instance = new SendMailManager();
                }
            }
            return instance;
        }

        public void SendEmail(string subject, string body){
           lock(this){
            try
            {
                MailMessage email = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                smtp.Host = AppConstant.EMAIL_SMTP_SERVER;

                // set up the Gmail server
                smtp.EnableSsl = true;
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(AppConstant.EMAIL_SENDER, AppConstant.EMAIL_PASSWORD);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


                // draft the email
                MailAddress fromAddress = new MailAddress(AppConstant.EMAIL_SENDER);
                email.From = fromAddress;
                email.To.Add(AppConstant.EMAIL_RECEIVER);
                email.Subject = subject;
                email.Body = body;

                smtp.Send(email);

                log.Debug("Success! Please check your e-mail.");
            }
            catch (Exception ex)
            {
                log.Error("Error: " + ex.ToString());
            }
           }
        }
    }
}
