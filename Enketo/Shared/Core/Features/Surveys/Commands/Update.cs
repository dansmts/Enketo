using Enketo.Shared.Core.Domain.Entities;
using Enketo.Shared.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Enketo.Shared.Core.Features.Surveys.Commands
{
    public sealed class Update
    {
        public sealed class Command : IRequest<Unit>
        {
            public Command(SurveyModel survey, string userId)
                => (Survey, UserId) = (survey, userId);

            public SurveyModel Survey { get; set; }
            public string UserId { get; set; }

            public sealed class SurveyModel
            {
                public string? Id { get; set; }
                public string? Name { get; set; }
                public string? Description { get; set; }
                public IEnumerable<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
                public string? CreatedBy { get; set; }

                public sealed class QuestionModel
                {
                    public string? Id { get; set; }
                    public string? Title { get; set; }
                    public QuestionType QuestionType { get; set; }
                    public DisplayType DisplayType { get; set; }
                    public IEnumerable<string> PossibleAnswers { get; set; } = new List<string>();
                    public DbAction DbAction { get; set; }
                }
            }
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(request => request.Survey)
                   .NotEmpty();

                RuleFor(request => request.Survey.Id)
                   .NotEmpty()
                   .NotNull();

                RuleFor(request => request.UserId)
                    .NotEmpty();
            }
        }

        public sealed class Handler : IRequestHandler<Command, Unit>
        {
            private readonly ILogger<Update> _logger;
            private readonly IMongoCollection<Survey> _db;

            public Handler(
                ILogger<Update> logger,
                IMongoDbContext ctx)
            {
                _logger = logger;
                _db = ctx.Surveys;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Check if user is authorized
                if (!request?.Survey?.CreatedBy?.Equals(request.UserId) ?? false)
                    throw new UnauthorizedAccessException();

                // Create filter to find survey to update
                var filter =
                    Builders<Survey>.Filter
                    .Where(s => s.Id == request.Survey.Id);

                // Create survey update definition
                var updateDefBuilder = Builders<Survey>.Update;
                var updateDef = updateDefBuilder.Combine(new UpdateDefinition<Survey>[]
                {
                    updateDefBuilder.Set(s => s.Name, request.Survey.Name),
                    updateDefBuilder.Set(s => s.Description, request.Survey.Description),
                    updateDefBuilder.Set(s => s.ModifiedOn, DateTime.UtcNow)
                });

                // Update survey in db
                await _db.FindOneAndUpdateAsync(filter, updateDef);

                // Add, update or delete questions
                foreach (var question in request.Survey.Questions)
                {
                    if (question?.DbAction == DbAction.NEW)
                    {
                        //var newQuestionDefBuilder =
                        //    Builders<Survey>.Update.Push(s => s.Questions, Question.New(
                        //        question.Title,
                        //        question.QuestionType,
                        //        question.DisplayType,
                        //        question.PossibleAnswers));

                        //await _db.FindOneAndUpdateAsync(filter, newQuestionDefBuilder);
                    }
                    else if (question?.DbAction == DbAction.UPDATE)
                    {
                        var updateQuestionDefBuilder =
                            Builders<Survey>.Update
                            .Set(s => s.Questions.Where(q => q.Id == question.Id).FirstOrDefault().Title, question.Title);

                        await _db.FindOneAndUpdateAsync(filter, updateQuestionDefBuilder);
                    }
                    else if (question?.DbAction == DbAction.DELETE)
                    {
                        var deleteQuestionDefBuilder =
                            Builders<Survey>.Update
                            .PullFilter(s => s.Questions, Builders<Question>.Filter.Eq(q => q.Id, question.Id));

                        await _db.FindOneAndUpdateAsync(filter, deleteQuestionDefBuilder);
                    }
                }

                // Return void
                return Unit.Value;
            }
        }
    }
}
