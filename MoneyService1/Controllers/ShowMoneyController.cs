using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableSettings;

namespace MoneyService1.Controllers
{
    [ApiController]
    [Authorize] 
    public class ShowMoneyController : ControllerBase
    {
        [Route("[controller]")]
        [HttpGet]
        public ActionResult<string> ShowMoney()
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));
            
            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));

                var scService = new ScoreService();
                var workerCount = 0;
                var oldCount = 1;
                int count = 2;
                string[] numStringNew = new string[count];
                string[] numString = new string[count];
                while (workerCount!=oldCount)
                {
                    try 
                    {
                        var showScore = scService.ReturnScore(new ScoreModel {ClientId = tokenGuid, Id = workerCount}, "SELECT * FROM viewscores WHERE clientid = @clientid AND id >= @id AND exist = TRUE;");
                        oldCount = workerCount;
                        workerCount = showScore.Id + 1;
                        numString = new string[count];
                        numStringNew[count-2] = showScore.NumScore;
                        numStringNew[count-1] = showScore.Cash.ToString();
                        for (int i = 0; i < count; i++)
                        {
                            numString[i] = numStringNew[i];
                        }

                        count=count+2;
                        numStringNew = new string[count];
                        for (int i = 0; i < count-2; i++)
                        {
                            numStringNew[i] = numString[i];
                        }
                    }
                    catch
                    {
                        workerCount = oldCount;
                    }
                }
                
                List<Scores> scores = new List<Scores>();
                for (int i = 0; i < count/2-1; i++)
                {
                    Scores num = new Scores() { Number = numString[i*2], Amount = numString[i*2+1] };
                    scores.Add(num);
                }
                return Ok(scores);
            }
            return BadRequest("No claim");
        }   
    }
    
    class Scores
    {
        public string Number { get; set; }
        public string Amount { get; set; }
    }
}