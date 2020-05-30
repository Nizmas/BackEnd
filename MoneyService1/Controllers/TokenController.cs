using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

using MoneyService1.DTO;
using Auth;
using TableSettings;

namespace MoneyService1.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class TokenController : ControllerBase
    {
        private IUserService _service;
        private AuthOptions _authOptions;
        public TokenController(IUserService service, IOptions<AuthOptions> authOptionsAccessor)
        {
            _service = service;
            _authOptions = authOptionsAccessor.Value;  
        }

        [HttpPost]
        public IActionResult Get([FromBody] UserCredentials user)
        {
            Console.WriteLine("Кто у нас тут?");
            if (_service.IsValidUser(user.UserName, user.Password)) 
            {
                var clService = new ClientService();
                var userGuid = clService.ReturnClient(new ClientModel{ClientName = user.UserName}, "SELECT * FROM clients WHERE clientname = @clientname;");
                var authClaims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName), 
                    new Claim(JwtRegisteredClaimNames.Jti, userGuid.UserGuid.ToString()), 
                };

                var token = new JwtSecurityToken(
                    issuer: _authOptions.Issuer, 
                    audience: _authOptions.Audience, 
                    expires: DateTime.Now.AddMinutes(_authOptions.ExpiresInMinutes), 
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SecureKey)),
                        SecurityAlgorithms.HmacSha256Signature)
                );

                if (_service.IsActiveUser(user.UserName))
                {
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                    });
                }
                return Unauthorized("Сontact the moderator!");
            }
            return Unauthorized("Check data, or create new account");
        }
    }
}