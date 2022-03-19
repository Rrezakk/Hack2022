using Hack2022.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hack2022.Services
{
    public class TestService
    {
        private readonly IMongoCollection<TestDataModel> _testModelCollection;

        public TestService(IOptions<TestDatabaseSettings> settings)
        {
            var MongoClient = new MongoClient(settings.Value.ConnectionString);
            _testModelCollection = MongoClient.GetDatabase(settings.Value.DatabaseName)
                .GetCollection<TestDataModel>(settings.Value.CollectionName);
        }

        public async Task<List<TestDataModel>> Get() => 
            await _testModelCollection.Find(_ => true).ToListAsync();

        public async Task Create(TestDataModel data) =>
            await _testModelCollection.InsertOneAsync(data);

        public async Task Update(string id, TestDataModel updateData) =>
            await _testModelCollection.ReplaceOneAsync(m => m.Id == id, updateData);

        public async Task Delete(string id) =>
            await _testModelCollection.DeleteOneAsync(m => m.Id == id);
    }
}
