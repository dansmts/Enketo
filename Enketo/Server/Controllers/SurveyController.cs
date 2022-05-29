using Enketo.Shared.Core.Features.Surveys.Commands;
using Enketo.Shared.Core.Features.Surveys.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enketo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SurveyController(IMediator mediator)
            => _mediator = mediator;

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var result = await _mediator.Send(new GetAll.Query(GetUserObjectId()));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAsync(string id)
        {
            try
            {
                var result = await _mediator.Send(new GetById.Query(id, GetUserObjectId()));
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> PostAsync(Create.Command.SurveyModel survey)
        {
            try
            {
                var result = await _mediator.Send(new Create.Command(survey, GetUserObjectId()));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //> await _db.UpsertSurveyAsync(survey);

        //[HttpPut("{id}")]
        //public async Task PutAsync(string id, [FromBody] UpsertSurveyDto survey)
        //    => await _db.UpsertSurveyAsync(id, survey);

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            try
            {
                var result = await _mediator.Send(new Delete.Command(id, GetUserObjectId()));
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetUserObjectId()
            => User.Claims.Where(c => c.Type.Contains("objectidentifier"))?.FirstOrDefault()?.Value ?? Guid.NewGuid().ToString();
    }
}
