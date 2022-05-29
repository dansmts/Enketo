using Enketo.Shared.Core.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using CreateQuestion = Enketo.Shared.Core.Features.Questions.Commands.Create;

namespace Enketo.Shared.Core.Features.Surveys.Commands
{
    public sealed class Create
    {
        public sealed class Command : IRequest<Unit>
        {
            public Command(SurveyModel survey, string userId)
                => (Survey, UserId) = (survey, userId);

            public SurveyModel Survey { get; set; }
            public string UserId { get; set; }

            public sealed class SurveyModel
            {
                public string? Name { get; set; }
                public string? Description { get; set; }
                public IEnumerable<CreateQuestion.Command> Questions { get; set; } = new List<CreateQuestion.Command>();
            }
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(request => request.Survey)
                   .NotEmpty();

                RuleFor(request => request.UserId)
                    .NotEmpty();
            }
        }

        public sealed class Handler : IRequestHandler<Command, Unit>
        {
            private readonly ILogger<Create> _logger;
            private readonly IMongoCollection<Survey> _db;
            private readonly IMediator _mediator;

            public Handler(
                ILogger<Create> logger,
                IMongoDbContext ctx,
                IMediator mediator)
            {
                _logger = logger;
                _db = ctx.Surveys;
                _mediator = mediator;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Map request model to Survey entity
                var survey = Survey.New(
                    request.Survey.Name,
                    request.Survey.Description,
                    request.UserId);

                // Add to db
                await _db.InsertOneAsync(survey, null, cancellationToken);

                // Add Questions to db
                var questionIds = new List<string>();
                foreach (var question in request.Survey.Questions)
                {
                    question.Question.CreatedBy = survey.CreatedBy;
                    question.Question.SurveyId = survey.Id;
                    questionIds.Add(await _mediator.Send(question, cancellationToken));
                }

                // Update survey with questionIds
                var filter =
                   Builders<Survey>.Filter
                   .Where(s => s.Id == survey.Id);

                var updateDefBuilder =
                    Builders<Survey>.Update
                    .Set(s => s.QuestionsList, questionIds);

                await _db.FindOneAndUpdateAsync(filter, updateDefBuilder, null, cancellationToken);

                // Return id
                return Unit.Value;
            }
        }
    }
}
