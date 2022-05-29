using Enketo.Shared.Core.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using DeleteQuestion = Enketo.Shared.Core.Features.Questions.Commands.Delete;

namespace Enketo.Shared.Core.Features.Surveys.Commands
{
    public sealed class Delete
    {
        public sealed class Command : IRequest<Unit>
        {
            public Command(string id, string userId)
                => (Id, UserId) = (id, userId);

            public string Id { get; set; }
            public string UserId { get; set; }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(request => request.Id)
                   .NotEmpty();

                RuleFor(request => request.UserId)
                    .NotEmpty();
            }
        }

        public sealed class Handler : IRequestHandler<Command, Unit>
        {
            private readonly ILogger<Delete> _logger;
            private readonly IMongoCollection<Survey> _db;
            private readonly IMediator _mediator;

            public Handler(
                ILogger<Delete> logger,
                IMongoDbContext ctx,
                IMediator mediator)
            {
                _logger = logger;
                _db = ctx.Surveys;
                _mediator = mediator;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Create filter to select survey by Id
                var filter =
                    Builders<Survey>.Filter
                    .Where(s => s.Id == request.Id);

                // Get survey from db
                var dbSurvey =
                    (await _db.FindAsync(filter, null, cancellationToken)).FirstOrDefault(cancellationToken);

                if (dbSurvey == null)
                    throw new ArgumentNullException();

                if (!dbSurvey?.CreatedBy?.Equals(request.UserId) ?? false)
                    throw new UnauthorizedAccessException();

                // Delete questions from db
                foreach (var question in dbSurvey.QuestionsList)
                    await _mediator.Send(new DeleteQuestion.Command(question, request.UserId), cancellationToken);

                // Delete survey from db
                await _db.DeleteOneAsync(filter, cancellationToken);

                // Return result
                return Unit.Value;
            }
        }
    }
}
