﻿using Cookbook.Api.Features.Auth;
using Cookbook.Api.Features.Auth.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cookbook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _mediator.Send(new Register(
                registerDto.UserName,
                registerDto.Password,
                registerDto.FirstName,
                registerDto.LastName));

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            string token = await _mediator.Send(new Login(loginDto.UserName, loginDto.Password));
            return Ok(new { token });
        }
    }
}