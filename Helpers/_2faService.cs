using System.Net.Mail;
using System.Net;

namespace vkr_bank.Helpers
{
    public class _2faService
    {
        public string GenerateCode()
        {
            Random rand = new Random();
            string result = null;
            for (int i = 0; i < 6; i++)
                result += rand.Next(10).ToString();
            return result;
        }
        public void SendCodeToMail(string code, string mail, string name)
        {
            MailAddress fromAddress = new MailAddress("4mykursach@gmail.com", "СкамКомБАНК");
            MailAddress toAddress = new MailAddress(mail, name);
            MailMessage message = new MailMessage(fromAddress, toAddress);
            message.Body = "Ваш код для подтверждения входа: " + code + Environment.NewLine + Environment.NewLine +
                "Если это не Вы, не отвечайте на это сообщение или удалите его.";
            message.Subject = "Вход в аккаунт СкамКомБАНК";

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            //smtpClient.Credentials = new NetworkCredential(fromAddress.Address, "4mykursachABOBA3");
            smtpClient.Credentials = new NetworkCredential(fromAddress.Address, "zzchloqfidedarqr");
            smtpClient.Send(message);
        }
    }
}
