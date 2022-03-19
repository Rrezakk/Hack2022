using System.Security.Cryptography;
using System.Text;
using Hack2022.Models;
using Hack2022.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hack2022.Services
{
    public class UserAccountsService
    {
        private readonly IMongoCollection<UserAccountModel> users;

        public UserAccountsService(IOptions<UserAccountsOptionsDB> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            users = mongoClient.GetDatabase(settings.Value.DatabaseName)
                .GetCollection<UserAccountModel>(settings.Value.CollectionName);
        }
        public IResult AddRole(string role, string googleId)
        {
            if (TryGetUserById(googleId, out var user))
            {
                user.role = role;
                UpdateUser(user);
                return Results.Ok($"sucessfully added role: {role} to {user.googleId}");
            }
            else
            {
                return Results.BadRequest($"error adding role {role} to {user.googleId}");
            }
        }
        public bool ContainsAccessedUser(string accessKey)
        {
            try
            {
                var user = users.FindSync((u) => u.acessToken == accessKey).First();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ContainsIdUser(string googleId)
        {
            try
            {
                var user = users.FindSync((u) => u.googleId == googleId).First();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool TryGetUserById(string googleId,out UserAccountModel user)
        {
            try
            {
                user = users.FindSync((u) => u.googleId == googleId).First();
                return true;
            }
            catch
            {
                user = null;
                return false;
            }
        }
        public bool TryGetUserByToken(string token,out UserAccountModel user)
        {
            try
            {
                user = users.FindSync((u) => u.acessToken == token).First();
                return true;
            }
            catch
            {
                user = null;
                return false;
            }
        }
        public void CreateUser(UserAccountModel user)
        {
            users.InsertOne(user);
        }
        public void UpdateUser(UserAccountModel user) => users.ReplaceOne(u => u.Id == user.Id, user);
        public bool CheckAccess(string token, string role)
        {
            var user = users.Find(e => e.acessToken == token).FirstOrDefault();
            return user != null && user.role == role;
        }
    }
}
