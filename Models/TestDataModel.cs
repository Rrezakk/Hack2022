using MongoDB.Bson.Serialization.Attributes;

namespace Hack2022.Models
{
    public class TestDataModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Keys { get; set; }
    }
}
