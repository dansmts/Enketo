using Enketo.Shared.Core.Domain.Entities;
using Enketo.Shared.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using static Enketo.Shared.Core.Features.Questions.Queries.GetById.Model;

namespace Enketo.Shared.Core.Features.Questions.Queries
{
    public sealed class GetById
    {
        public sealed class Query : IRequest<Model>
        {
            public Query(string id, string userId)
                => (Id, UserId) = (id, userId);

            public string Id { get; set; }
            public string UserId { get; set; }
        }
        public sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(request => request.Id)
                   .NotEmpty();

                RuleFor(request => request.UserId)
                    .NotEmpty();
            }
        }

        public sealed class Handler : IRequestHandler<Query, Model>
        {
            private readonly ILogger<GetById> _logger;
            private readonly IMongoCollection<Question> _db;

            public Handler(
                ILogger<GetById> logger,
                IMongoDbContext ctx)
            {
                _logger = logger;
                _db = ctx.Questions;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                // Create filter to select question by Id
                var filter =
                    Builders<Question>.Filter
                    .Where(q => q.Id == request.Id);

                // Get question from db
                var dbQuestion =
                    (await _db.FindAsync(filter)).FirstOrDefault();

                if (dbQuestion == null)
                    throw new ArgumentException();

                if (!dbQuestion?.CreatedBy?.Equals(request.UserId) ?? false)
                    throw new UnauthorizedAccessException();


                // Map question to return model
                var question = new QuestionModel()
                {
                    Id = dbQuestion?.Id,
                    Title = dbQuestion?.Title,
                    QuestionType = dbQuestion.QuestionType,
                    DisplayType = dbQuestion.DisplayType,
                    PossibleAnswers = dbQuestion?.PossibleAnswers,
                    Responses = dbQuestion?.Responses,
                    ModifiedOn = dbQuestion?.ModifiedOn
                };


                // Return result
                return new Model() { Question = question };
            }
        }

        public sealed class Model
        {
            public QuestionModel? Question { get; set; }

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
