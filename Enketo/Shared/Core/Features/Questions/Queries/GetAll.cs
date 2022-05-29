using Enketo.Shared.Core.Domain.Entities;
using Enketo.Shared.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using static Enketo.Shared.Core.Features.Questions.Queries.GetAll.Model;

namespace Enketo.Shared.Core.Features.Questions.Queries
{
    public sealed class GetAll
    {
        public sealed class Query : IRequest<Model>
        {
            public Query(string userId)
                => UserId = userId;

            public Query(string surveyId, string userId)
                => (SurveyId, UserId) = (surveyId, userId);

            public string SurveyId { get; set; }
            public string UserId { get; set; }

        }
        public sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(request => request.UserId)
                    .NotEmpty();
            }
        }

        public sealed class Handler : IRequestHandler<Query, Model>
        {
            private readonly ILogger<GetAll> _logger;
            private readonly IMongoCollection<Question> _db;

            public Handler(
                ILogger<GetAll> logger,
                IMongoDbContext ctx)
            {
                _logger = logger;
                _db = ctx.Questions;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                // Create filter to only select questions created by logged in user
                var filter = string.IsNullOrEmpty(request.SurveyId) ?
                    Builders<Question>.Filter.Where(q => q.CreatedBy == request.UserId)
                    : Builders<Question>.Filter.Where(q => q.SurveyId == request.SurveyId && q.CreatedBy == request.UserId);

                // Get questions from db
                var dbQuestions =
                    (await _db.FindAsync(filter)).ToList();

                // Map questions to return model
                var questions = new List<QuestionModel>();
                dbQuestions.ForEach(q => questions.Add(new QuestionModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    QuestionType = q.QuestionType,
                    DisplayType = q.DisplayType,
                    PossibleAnswers = q.PossibleAnswers,
                    Responses = q.Responses,
                    ModifiedOn = q.ModifiedOn
                }));

                // Return result
                return new Model() { Questions = questions };
            }
        }

        public sealed class Model
        {
            public IEnumerable<QuestionModel> Questions { get; set; }

            public sealed class QuestionModel
            {
                public string? Id { get; set; }
                public string? Title { get; set; }
                public QuestionType QuestionType { get; set; }
                public DisplayType DisplayType { get; set; }
                public IEnumerable<string>? PossibleAnswers { get; set; } = new List<string>();
                public IEnumerable<Response>? Responses { get; set; } = new List<Response>();
                public DateTime? ModifiedOn { get; set; }
            }
        }
    }
}
