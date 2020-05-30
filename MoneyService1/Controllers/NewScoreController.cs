using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableSettings;

namespace MoneyService1.Controllers
{
    [ApiController]
    [Authorize]
    public class NewScoreController : ControllerBase
    {
        [Route("[controller]")]
        [HttpGet]
        public ActionResult CreateScore()
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var scService = new ScoreService();
                var randomNumber = "4";
                int i = 1;
                while (i<10)
                {
                    Random rnd = new Random();
                    randomNumber = randomNumber + rnd.Next(0, 10);
                    if (scService.ReturnScore(new ScoreModel {NumScore = randomNumber}, "SELECT * FROM scores WHERE numscore = @numscore AND exist = TRUE;") != null && i == 9 )
                    {
                        randomNumber = "4";
                        i = 0;
                    }
                    
                    if (scService.ReturnScore(new ScoreModel {NumScore = randomNumber}, "SELECT * FROM scores WHERE numscore = @numscore AND exist = TRUE;") == null && i == 9 )
                    {
                        scService.WorkScore(new ScoreModel {ClientId = tokenGuid, NumScore = randomNumber}, "INSERT INTO scores (clientid, numscore) VALUES (@clientid, @numscore);");
                    }
                    i++;
                }
                return Ok("New score is created!");
            }
            return BadRequest("Something wrong");
        }
    }
}