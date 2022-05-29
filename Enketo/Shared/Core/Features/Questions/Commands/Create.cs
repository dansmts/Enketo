using Enketo.Shared.Core.Domain.Entities;
using Enketo.Shared.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Enketo.Shared.Core.Features.Questions.Commands
{
    public sealed class Create
    {
        public sealed class Command : IRequest<string>
        {
            public Command(QuestionModel question)
                => Question = question;

            public QuestionModel Question { get; set; }


            public sealed class QuestionModel
            {
                public string? Title { get; set; }
                public QuestionType QuestionType { get; set; }
                public DisplayType DisplayType { get; set; }
                public IEnumerable<string> PossibleAnswers { get; set; } = new List<string>();
                public string? CreatedBy { get; set; }
                public string? SurveyId { get; set; }
            }
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(request => request.Question)
                   .NotEmpty();
            }
        }

        public sealed class Handler : IRequestHandler<Command, string>
        {
            private readonly ILogger<Create> _logger;
            private readonly IMongoCollection<Question> _db;

            public Handler(
                ILogger<Create> logger,
                IMongoDbContext ctx)
            {
                _logger = logger;
                _db = ctx.Questions;
            }

            public async Task<string> Handle(Command request, CancellationToken cancellationToken)
            {
                // Map request model to question entity
                var question = Question.New(
                    request.Question.Title,
                    request.Question.QuestionType,
                    request.Question.DisplayType,
                    request.Question.PossibleAnswers,
                    request.Question.CreatedBy,
                    request.Question.SurveyId);

                // Add to db
                await _db.InsertOneAsync(question, null, cancellationToken);

                // Return id
                return question.Id ?? string.Empty;
            }
        }
    }
}
