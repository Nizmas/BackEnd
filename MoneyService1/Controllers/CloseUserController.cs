using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MoneyService1.DTO;
using TableSettings;

namespace MoneyService1.Controllers
{
    /// <summary>
    ///  Контроллер для закрытия клиента. Фактически происходит лишь перевод в неактивную фазу изменение параметра exist
    /// для подтверждения действия необходимо ввести реальное имя
    /// </summary>
    [ApiController]
    [Authorize]
    public class CloseUserController : ControllerBase
    {
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] UserCredentials oldUser)
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                Console.WriteLine(tokenGuid);
                var clService = new ClientService();
                var checkName = clService.ReturnClient(new ClientModel {UserGuid = tokenGuid}, "SELECT * FROM clients WHERE userguid = @userguid;");
                Console.WriteLine(checkName.RealName);
                Console.WriteLine(oldUser.RealName);
                if (Equals (checkName.RealName, oldUser.RealName))
                {
                    clService.WorkClient(new ClientModel {UserGuid = tokenGuid, RealName = oldUser.RealName}, "UPDATE clients SET clientstatus = false WHERE userguid = @userguid AND realname = @realname;");
                    return Ok("User was unactivated");
                }
                return BadRequest("Name is false");
            }
            return BadRequest("Try to enter one more time");
        }
        
    }
}