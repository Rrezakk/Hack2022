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
        private readonly IMongoCollection<SubscriberModel> subscribers;
        public NotificationService(IOptions<NotificationDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            notifications = mongoClient.GetDatabase(settings.Value.DatabaseName).
                GetCollection<NotificationModel>(settings.Value.CollectionName);
            subscribers = mongoClient.GetDatabase(settings.Value.DatabaseName).
                GetCollection<SubscriberModel>(settings.Value.Collection2Name);
        }
        public List<NotificationPendingModel> GetAllNotificationsToPending()
        {
            return notifications.Find(_ => true).ToList().Select(e=>new NotificationPendingModel(e)).ToList();
        }
        public List<NotificationModel> GetNotificationsByGoogleId(string googleId)
        {
            return notifications.Find(e => e.RecepientGoogleId==googleId).ToList();
        }

        public void ExecuteNotification(NotificationModel n)
        {
            Console.WriteLine("executing: " + JsonSerializer.Serialize(n));
            
        }
        public void CreateNotification(NotificationModel n)
        {
            Console.WriteLine("adding: "+JsonSerializer.Serialize(n));
            n.Id = null;
            notifications.InsertOne(n);
        }
        public void RemoveNotificationById(string? id) => notifications.DeleteOne((n) => n.Id == id);
        public void RemoveAllNotificationsByIds(List<string> ids) =>
            notifications.DeleteMany((e) => ids.Contains(e.RecepientGoogleId));

        public List<string>? GetSubscribedTags(string googleId)
        {
            var tags = subscribers.Find(s => s.googleId== googleId).ToList().ConvertAll<List<string>>(s => s.subscribedTags).FirstOrDefault();
            return tags;
        }
        public IResult SubscribeTag(string googleId, string tagName)
        {
            Console.WriteLine($"{googleId} subscribe to {tagName}");
            var subscriber = subscribers.FindSync(e => e.googleId == googleId).FirstOrDefault();
            if (subscriber == null)
            {
                subscriber = new SubscriberModel
                {
                    googleId = googleId,
                    subscribedTags = new List<string>(){tagName}
                };
                subscribers.InsertOne(subscriber);
                return Results.Ok("added");
            }
            else
            {
                if (subscriber.subscribedTags.Contains(tagName))
                {
                    subscriber.subscribedTags.Remove(tagName);
                    subscribers.ReplaceOne(e => e.googleId == googleId, subscriber);
                    return Results.Ok("removed");
                }
                else
                {
                    subscriber.subscribedTags.Add(tagName);
                    subscribers.ReplaceOne(e => e.googleId == googleId, subscriber);
                    return Results.Ok("added");
                }
            }

            
        }
        public bool CheckUserSubscribedTag(string googleId, string tagName)
        {
            var subscriber = subscribers.FindSync(e => e.googleId == googleId).FirstOrDefault();
            if (subscriber == null)
            {
                return false;
            }
            else
            {
                if (subscriber.subscribedTags.Contains(tagName))
                {
                    return true;
                }
                else
                {
                    subscriber.subscribedTags.Add(tagName);
                    return false;
                }
            }
        }
    }
}
