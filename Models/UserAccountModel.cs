using MongoDB.Bson.Serialization.Attributes;

namespace Hack2022.Models
{
    public class UserAccountModel
    {
        public UserAccountModel(){}

        public UserAccountModel(string googleId,string role)
        {
            this.googleId=googleId;
            this.role = role;
        }
        public void AssignToken(string accessToken) => this.acessToken = acessToken;
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        public string googleId { get; set; } = "";
        public string acessToken { get; private set; } = "";
        public string role { get; set; } = "";
        public bool CheckAccess(string role) => this.role==role;
    }
}
