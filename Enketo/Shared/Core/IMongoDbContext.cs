using Enketo.Shared.Core.Domain.Entities;
using MongoDB.Driver;

namespace Enketo.Shared.Core
{
    public interface IMongoDbContext
    {
        IMongoClient Client { get; }

        IMongoDatabase Database { get; }

        IMongoCollection<Survey> Surveys { get; }
        IMongoCollection<Question> Questions { get; }
    }
}
