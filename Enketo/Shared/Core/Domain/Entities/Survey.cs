using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Enketo.Shared.Core.Domain.Entities
{
    public sealed class Survey
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Code { get; private set; }
        public bool Published { get; private set; } = false;
        public IEnumerable<string> QuestionsList { get; private set; } = new List<string>();

        [BsonIgnore]
        public IEnumerable<Question> Questions { get; private set; } = new List<Question>();

        public string? CreatedBy { get; private set; }
        public DateTime? ModifiedOn { get; private set; }

        public Survey() { }

        private Survey(
            string? id,
            string? name,
            string? description,
            string? createdBy)
        {
            Id = id;
            Name = name;
            Description = description;
            CreatedBy = createdBy;

            ModifiedOn = DateTime.UtcNow;
        }

        public static Survey New(
            string? name,
            string? description,
            string? createdBy)
            => new(null, name, description, createdBy);
    }
}
