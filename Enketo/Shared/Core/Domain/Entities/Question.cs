using Enketo.Shared.Core.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Enketo.Shared.Core.Domain.Entities
{
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; private set; }
        public string? Title { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public DisplayType DisplayType { get; private set; }
        public IEnumerable<string> PossibleAnswers { get; private set; } = new List<string>();
        public IEnumerable<Response> Responses { get; private set; } = new List<Response>();

        public string? CreatedBy { get; private set; }
        public DateTime? ModifiedOn { get; private set; }

        public string? SurveyId { get; private set; }

        public Question() { }

        private Question(
            string? id,
            string? title,
            QuestionType questionType,
            DisplayType displayType,
            IEnumerable<string> possibleAnswers,
            string? createdBy,
            string? surveyId)
        {
            Id = id;
            Title = title;
            QuestionType = questionType;
            DisplayType = displayType;
            PossibleAnswers = possibleAnswers;

            CreatedBy = createdBy;
            SurveyId = surveyId;

            ModifiedOn = DateTime.UtcNow;
        }

        public static Question New(
            string? title,
            QuestionType questionType,
            DisplayType displayType,
            IEnumerable<string> possibleAnswers,
            string? createdBy,
            string? surveyId)
            => new(null, title, questionType, displayType, possibleAnswers, createdBy, surveyId);
    }
}
