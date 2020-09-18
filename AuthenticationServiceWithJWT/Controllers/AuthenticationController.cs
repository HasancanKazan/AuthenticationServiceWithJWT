using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationServiceWithJWT.Context;
using AuthenticationServiceWithJWT.Models;
using AuthenticationServiceWithJWT.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationServiceWithJWT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationDbContext _context;
        private readonly ITokenBuilder _tokenBuilder;

        public AuthenticationController(AuthenticationDbContext _context,ITokenBuilder _tokenBuilder)
        {
            this._context = _context;
            this._tokenBuilder = _tokenBuilder;
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var dbUser = await _context
                .Users
                .SingleOrDefaultAsync(u => u.UserName == user.UserName);

            if (dbUser == null)
            {
                return NotFound("User not found.");
            }

            var isValid = dbUser.Password == user.Password;

            if (!isValid)
            {
                return BadRequest("Could not authenticate user.");
            }

            var token = _tokenBuilder.BuildToken(user.UserName);

            return Ok(token);
        }


        [HttpGet("verify")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> VerifyToken()
        {
            var username = User
                .Claims
                .SingleOrDefault();

            if (username == null)
            {
                return Unauthorized();
            }

            var userExists = await _context
                .Users
                .AnyAsync(u => u.UserName == username.Value);

            if (!userExists)
            {
                return Unauthorized();
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("test")]
        public bool Test()
        {
            return true;
        }
    }   
}
