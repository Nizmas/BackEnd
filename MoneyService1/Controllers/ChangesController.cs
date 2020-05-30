using System;
using System.Linq;
using System.Text.RegularExpressions;
using Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MoneyService1.DTO;
using TableSettings;

namespace MoneyService1.Controllers
{
    [ApiController]
    [Authorize]
    public class ChangeController: ControllerBase
    {
        [Route("[controller]/data")]
        [HttpPost]
        public IActionResult Get([FromBody] UserCredentials change)
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var clService = new ClientService();
                if (change.RealName.Length > 3)
                {
                    clService.WorkClient(new ClientModel {RealName = change.RealName, UserGuid = tokenGuid},
                        "UPDATE clients SET realname = @realname WHERE userguid = @userguid;");
                    return Ok("Successfully changed!");
                }
                
                if (Regex.Match(change.UserName.ToLower(), pattern, RegexOptions.IgnoreCase).Success)
                {
                    var expectedClient = clService.ReturnClient(new ClientModel{ClientName = change.UserName}, "SELECT * FROM clients WHERE clientname = @clientname;");
                    if (expectedClient == null)
                    {
                        clService.WorkClient(new ClientModel {ClientName = change.UserName, UserGuid = tokenGuid},
                            "UPDATE clients SET clientname = @clientname WHERE userguid = @userguid;");
                        return Ok("Successfully changed!");
                    }
                    return BadRequest("Insert mail address!");
                }
                return BadRequest("Spaces are empties!");
            }
            return BadRequest("Authorise one more time!");
        }

        [Route("[controller]/password")]
        [HttpPost]
        public IActionResult Change([FromBody] UserCredentials pass)
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));
            
            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                if (Equals(pass.Password, pass.PasswordCheck))
                {
                    var clService = new ClientService();
                    var newPass = new Password(pass.Password);
                    clService.WorkClient(
                        new ClientModel
                            {PasswordHash = newPass.PasswordHash, PasswordSalt = newPass.Salt, UserGuid = tokenGuid},
                        "UPDATE clients SET passwordhash = @passwordhash, passwordsalt = @passwordsalt WHERE userguid = @userguid;");
                    return Ok("Successfully changed!");
                }
                return BadRequest("Check passwords!");
            }
            return BadRequest("Authorize one more time!");
        }
    }
}