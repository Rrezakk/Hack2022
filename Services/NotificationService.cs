using System.Text.Json;
using Hack2022.Models;
using Hack2022.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hack2022.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<NotificationModel> notifications;

        public NotificationService(IOptions<NotificationDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            notifications = mongoClient.GetDatabase(settings.Value.DatabaseName)
                .GetCollection<NotificationModel>(settings.Value.CollectionName);
        }
        public List<NotificationModel> GetAllNotifications()
        {
            return notifications.Find(_ => true).ToList();
        }
        public List<NotificationModel> GetNotificationsByGoogleId(string googleId)
        {
            return notifications.Find(e => e.RecepientGoogleId==googleId).ToList();
        }
        public void CreateNotification(NotificationModel n)
        {
            Console.WriteLine("adding: "+JsonSerializer.Serialize(n));
            n.Id = null;
            notifications.InsertOne(n);
        }
        public void RemoveNotificationById(string id) => notifications.DeleteOne((n) => n.Id == id);
        public void RemoveAllNotificationsByIds(List<string> ids) =>
            notifications.DeleteMany((e) => ids.Contains(e.RecepientGoogleId));
    }
}
