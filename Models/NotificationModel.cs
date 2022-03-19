using MongoDB.Bson.Serialization.Attributes;

namespace Hack2022.Models
{
    public class NotificationModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Text { get; set; }
        public string RecepientGoogleId { get; set; }
        public DateTime time { get; set; }
    }
}
