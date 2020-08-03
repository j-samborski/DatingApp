using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        //injecting config (appsetings and repo)
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            this._config = config;
            this._repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDTO userForRegisterDTO) //[FroomBody opcjonalnie]
        {

            userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDTO.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDTO.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDTO.Password);

            //to będzie później, trzeba będzie podać ścieżkę
            //  return CreatedAtRoute()
            return StatusCode(201); //oszukaństwo na razie
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // if (!await _repo.UserExists(userForLoginDto.Username)){
            //     return Unauthorized();
            // }

            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (userFromRepo == null)
                return Unauthorized();
            //budowanie tokena, building 2 claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };
            //pobranie symetrycznego klucza z settingów
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("Appsettings:Token").Value));
            //hashowanie crenedtiali
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //payload
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds

            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new{
                token = tokenHandler.WriteToken(token)
            });
        }
    }

}
