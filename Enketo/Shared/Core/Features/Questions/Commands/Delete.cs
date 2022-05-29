using Enketo.Shared.Core.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Enketo.Shared.Core.Features.Questions.Commands
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
            private readonly IMongoCollection<Question> _db;

            public Handler(
                ILogger<Delete> logger,
                IMongoDbContext ctx)
            {
                _logger = logger;
                _db = ctx.Questions;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Create filter to select survey by Id
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

                // Delete survey from db
                await _db.DeleteOneAsync(filter);

                // Return result
                return Unit.Value;
            }
        }
    }
}
