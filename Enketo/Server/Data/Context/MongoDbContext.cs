using Enketo.Server.Data.Settings;
using Enketo.Shared.Core;
using Enketo.Shared.Core.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Enketo.Server.Data.Context
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> dbOptions)
        {
            var settings = dbOptions.Value;
            _client = new MongoClient(settings.ConnectionString);
            _database = _client.GetDatabase(settings.DatabaseName);
        }

        public IMongoClient Client => _client;

        public IMongoDatabase Database => _database;

        public IMongoCollection<Survey> Surveys => _database.GetCollection<Survey>(MongoDbCollectionNames.Surveys);

        public IMongoCollection<Question> Questions => _database.GetCollection<Question>(MongoDbCollectionNames.Questions);
    }
}
