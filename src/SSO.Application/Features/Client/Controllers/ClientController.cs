using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSO.Application.Features.Client.Commands;
using SSO.Application.Features.Client.Queries;
using SSO.Application.Features.Client.Requests;

namespace SSO.Application.Features.Client.Controllers
{
    [Authorize(Roles = "OWNER")]
    public class ClientController : Controller
    {
        private IMapper _mapper;
        private readonly IMediator _mediator;
        private ILogger<ClientController> _logger;

        public ClientController(
            IMapper mapper
            , IMediator mediator
            , ILogger<ClientController> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index(GetClientRequest request)
        {
            var query = _mapper.Map<GetClientQuery>(request);
            var result = await _mediator.Send(query);
            return View("~/Features/Client/Pages/Index.cshtml", result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(GetClientInfoRequest request)
        {
            var query = _mapper.Map<GetClientInfoQuery>(request);
            var result = await _mediator.Send(query);
            return View("~/Features/Client/Pages/Edit.cshtml", result);
        }

        [HttpPost]
        public async Task<IActionResult> SetClientSecret(SetClientSecretRequest request)
        {
            var command = _mapper.Map<SetClientSecretCommand>(request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
