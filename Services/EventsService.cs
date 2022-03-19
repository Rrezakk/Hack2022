using Hack2022.Models;
using Hack2022.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hack2022.Services
{
    public class EventsService
    {
        private readonly IMongoCollection<EventModel> events;

        public EventsService(IOptions<EventsDBSettings> settings)
        {
            var MongoClient = new MongoClient(settings.Value.ConnectionString);
            events = MongoClient.GetDatabase(settings.Value.DatabaseName)
                .GetCollection<EventModel>(settings.Value.CollectionName);
        }

        public List<EventModel> GetAll() => events.Find(_ => true).ToList();
        public EventModel GetById(string eventId) => events.Find(e => e.Id == eventId).FirstOrDefault();
        public void AddRange(List<EventModel> e) => events.InsertMany(e);
        public void RemoveById(string eventId) => events.DeleteOne(e => e.Id == eventId);
        public void Update(string EventId, EventModel ev) => events.ReplaceOne(e => e.Id == EventId, ev);
    }
}
