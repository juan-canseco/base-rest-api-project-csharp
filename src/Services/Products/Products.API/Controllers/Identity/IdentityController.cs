using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Features.Identity.Auth.Commands;

namespace Products.API.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IdentityController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> GetTokenAsync(GetTokenCommand command)
        {
            var token = await _mediator.Send(command);
            return Ok(token);
        }

    }
}
