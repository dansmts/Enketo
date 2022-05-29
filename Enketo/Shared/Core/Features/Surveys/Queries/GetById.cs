using Enketo.Shared.Core.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using static Enketo.Shared.Core.Features.Surveys.Queries.GetById.Model;
using GetQuestion = Enketo.Shared.Core.Features.Questions.Queries.GetAll;

namespace Enketo.Shared.Core.Features.Surveys.Queries
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
            private readonly IMongoCollection<Survey> _db;
            private readonly IMediator _mediator;

            public Handler(
                ILogger<GetById> logger,
                IMongoDbContext ctx,
                IMediator mediator)
            {
                _logger = logger;
                _db = ctx.Surveys;
                _mediator = mediator;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                // Create filter to select survey by Id
                var filter =
                    Builders<Survey>.Filter
                    .Where(s => s.Id == request.Id);

                // Get survey from db
                var dbSurvey =
                    (await _db.FindAsync(filter)).FirstOrDefault();

                if (dbSurvey == null)
                    throw new ArgumentException();

                if (!dbSurvey?.CreatedBy?.Equals(request.UserId) ?? false)
                    throw new UnauthorizedAccessException();

                // Get questions for survey
                var getAllQuestionsModel = await _mediator.Send(new GetQuestion.Query(request.Id, request.UserId));

                // Map survey to return model
                var survey = new SurveyModel()
                {
                    Id = dbSurvey.Id,
                    Name = dbSurvey.Name,
                    Description = dbSurvey.Description,
                    Code = dbSurvey.Code,
                    Published = dbSurvey.Published,
                    Questions = getAllQuestionsModel.Questions,
                    ModifiedOn = dbSurvey.ModifiedOn,
                };

                // Return result
                return new Model() { Survey = survey };
            }
        }

        public sealed class Model
        {
            public SurveyModel? Survey { get; set; }

            public sealed class SurveyModel
            {
                public string? Id { get; set; }
                public string? Name { get; set; }
                public string? Description { get; set; }
                public string? Code { get; set; }
                public bool Published { get; set; } = false;
                public IEnumerable<GetQuestion.Model.QuestionModel> Questions { get; set; }
                    = new List<GetQuestion.Model.QuestionModel>();
                public DateTime? ModifiedOn { get; set; }
            }
        }
    }
}
