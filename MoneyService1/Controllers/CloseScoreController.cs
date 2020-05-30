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
    ///  Контроллер для закрытия счёта. Фактически происходит лишь перевод в неактивную фазу изменение параметра exist
    /// </summary>
    [ApiController]
    [Authorize]
    public class CloseScoreController : ControllerBase
    {
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] ScoreCredentials oldScore)
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                
                var scService = new ScoreService();
                scService.WorkScore(new ScoreModel {ClientId = tokenGuid, NumScore = oldScore.ScoreFrom}, "UPDATE scores SET exist = false WHERE clientid = @clientid AND numscore = @numscore;");
                return Ok("Score closed");
            }
            return BadRequest("Try to enter one more time");
        }
        
    }
}