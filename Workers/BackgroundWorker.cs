using System.Net;
using System.Net.Mail;
using Hack2022.Services;

namespace Hack2022.Workers
{
    public class BackgroundWorker
    {
        private readonly NotificationService _notificationService;
        public BackgroundWorker(NotificationService notificationService)
        {
            this._notificationService = notificationService;
            Task.Run(()=> Cycle());
        }
        private async Task Cycle()
        {
            while (true)
            {
                ProcessNotifications();
                Thread.Sleep(1000);
            }
        }

        void ProcessNotifications()
        {
            Console.WriteLine("step");
            var now = new DateTimeOffset(DateTime.UtcNow);
            var notifications = _notificationService.GetAllNotificationsToPending();
            foreach (var v in notifications)
            {
                if (v.utcTimeWithOffset > now.ToUnixTimeSeconds())
                {
                    //Email($"Notification: {v.Text}", "abrabokamoke@gmail.com");

                    Console.WriteLine($"Expired: {v.Id} {v.Tittle} {v.Description}");
                    var view = v.ToViewModel();
                    Console.WriteLine($"view: {v.Id} {v.Tittle} {v.Description}");
                    //executing notification

                    _notificationService.RemoveNotificationById(v.Id);
                }
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
