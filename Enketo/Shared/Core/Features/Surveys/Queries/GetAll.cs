using Enketo.Shared.Core.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using static Enketo.Shared.Core.Features.Surveys.Queries.GetAll.Model;

namespace Enketo.Shared.Core.Features.Surveys.Queries
{
    public sealed class GetAll
    {
        public sealed class Query : IRequest<Model>
        {
            public Query(string userId)
                => UserId = userId;

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
            private readonly IMongoCollection<Survey> _db;

            public Handler(
                ILogger<GetAll> logger,
                IMongoDbContext ctx)
            {
                _logger = logger;
                _db = ctx.Surveys;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                // Create filter to only select surveys created by logged in user
                var filter =
                    Builders<Survey>.Filter
                    .Where(s => s.CreatedBy == request.UserId);

                // Get surveys from db
                var dbSurveys =
                    (await _db.FindAsync(filter)).ToList();

                // Map surveys to return model
                var surveys = new List<SurveyModel>();
                dbSurveys.ForEach(s => surveys.Add(new SurveyModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Code = s.Code,
                    Published = s.Published,
                    ModifiedOn = s.ModifiedOn
                }));

                // Return result
                return new Model() { Surveys = surveys };
            }
        }

        public sealed class Model
        {
            public IEnumerable<SurveyModel> Surveys { get; set; } = new List<SurveyModel>();

            public sealed class SurveyModel
            {
                public string? Id { get; set; }
                public string? Name { get; set; }
                public string? Description { get; set; }
                public string? Code { get; set; }
                public bool Published { get; set; } = false;
                public DateTime? ModifiedOn { get; set; }
            }
        }
    }
}
