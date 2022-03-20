using MongoDB.Bson.Serialization.Attributes;

namespace Hack2022.Models
{
    public class SubscriberModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string googleId { get; set; }
        public List<string> subscribedTags { get; set; }
    }
}
