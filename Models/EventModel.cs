using MongoDB.Bson.Serialization.Attributes;

namespace Hack2022.Models
{
    public class EventModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ShortDesc { get; set; }
        public string Desc { get; set; }
        public DateTime time { get; set; }
        public int notifyPredelayMinutes { get; set; } = 15;//
        public string chatLink { get; set; } = "";
        public bool selfDeleting { get; set; } = true;
        public int notifyState { get; set; } = 0;
        public long views { get; set; } = 0;
        public long linkFollows { get; set; } = 0;
        public long subsCount { get; set; } = 0;
        public List<string> subscribersIds { get; set; } = new List<string>();
        public List<string> tags { get; set; }=new List<string>();

    }
}
