using System.Net;
using System.Net.Mail;
using Hack2022.Services;

namespace Hack2022.Workers
{
    public class BackgroundWorker
    {
        private NotificationService s;
        private string id;
        private bool isRunning = false;
        public BackgroundWorker(NotificationService service,string id)
        {
            s = service;
            id = id;
        }
        public void Run()
        {
            if (!isRunning)
            {
                Task.Run(Cycle);
                isRunning = true;
            }
        }

        async Task Cycle()
        {
            var Date = new DateTime(DateTime.Now.Millisecond);
            while (true)
            {
               var c = s.GetNotificationsByGoogleId(id);
                foreach (var v in c)
                {
                    if (v.time > Date)
                    {
                        Email($"Notification: {v.Text}", "abrabokamoke@gmail.com");
                        s.RemoveNotificationById(v.Id);
                    }
                }

                Thread.Sleep(1000);
            }
        }
        void Email(string htmlString, string mail)
        {
            try
            {
                // отправитель - устанавливаем адрес и отображаемое в письме имя
                MailAddress from = new MailAddress("kirill.reznikov.2011107@gmail.com", "Tom");
                // кому отправляем
                MailAddress to = new MailAddress(mail);
                // создаем объект сообщения
                MailMessage m = new MailMessage(from, to);
                // тема письма
                m.Subject = "Тест";
                // текст письма
                m.Body = htmlString;
                // письмо представляет код html
                m.IsBodyHtml = false;
                // адрес smtp-сервера и порт, с которого будем отправлять письмо
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                // логин и пароль
                smtp.Credentials = new NetworkCredential("abrabokamoke@gmail.com", "abrabokamoke123");
                smtp.EnableSsl = true;
                smtp.Send(m);


            }
            catch (Exception e) { Console.WriteLine(e); }
        }
    }
}
