using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MoneyService1.DTO;
using TableSettings;

namespace MoneyService1.Controllers
{
    public class TransferMoneyController : ControllerBase
    {
        [Route("[controller]")]
        [HttpPost]
        public ActionResult Get([FromBody] ScoreCredentials score)
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));

                if (score.ScoreFrom == "AddMoney") // Если вместо сходного счёта эта строка, то деньги добавляем из буферного счёта для сторонних поступлений
                {
                    tokenGuid = new Guid("f414ec5a-a585-4368-beec-6b488cf76b51"); 
                    score.ScoreFrom = "4000000000";
                }
                var istemplate = false;
                var scService = new ScoreService();
                var clService = new ClientService();
                var histService = new HistoryService();
                var takeScore = scService.ReturnScore(new ScoreModel {ClientId = tokenGuid, NumScore = score.ScoreFrom}, "SELECT * FROM scores WHERE clientid = @clientid AND numscore = @numscore AND exist = TRUE;");
                var cashLess = takeScore.Cash; 
                float cashMore;
                try
                {
                    var showScore = scService.ReturnScore(new ScoreModel {NumScore = score.ScoreTo}, "SELECT * FROM scores WHERE numscore = @numscore AND exist = TRUE;");
                    cashMore = showScore.Cash;
                }
                catch
                {
                    return BadRequest("Check number");
                }

                if (cashLess >= score.HowMuch) // проверить соответствие принимающей стороны
                {
                    var takerId = clService.ReturnClient(new ClientModel {ClientName = score.TakerName},
                        "SELECT * FROM clients WHERE clientname = @clientname");

                    if (scService.ReturnScore(new ScoreModel{NumScore = score.ScoreTo, ClientId = takerId.UserGuid}, "SELECT * FROM scores WHERE clientid = @clientid AND numscore = @numscore AND exist = TRUE;")!=null) // проверить, принадлежит ли номер счёта принимателю
                    {
                        cashLess = cashLess - score.HowMuch;
                        cashMore = cashMore + score.HowMuch;
                        scService.WorkScore(new ScoreModel {NumScore = score.ScoreFrom, Cash = cashLess, ClientId = tokenGuid}, "UPDATE scores SET cash = @cash WHERE clientid = @clientid AND numscore = @numscore;");
                        scService.WorkScore(new ScoreModel {NumScore = score.ScoreTo, Cash = cashMore, ClientId = takerId.UserGuid}, "UPDATE scores SET cash = @cash WHERE clientid = @clientid AND numscore = @numscore AND exist = TRUE;");
                        if (score.IsTemplate == "true") istemplate = true;

                        histService.WorkHistory(new HistoryModel {ScoreFrom = score.ScoreFrom, ScoreTo = score.ScoreTo,  HowMuch = score.HowMuch, ClientId = tokenGuid, TakerId = takerId.UserGuid, Template = istemplate}, 
                            "INSERT INTO histories(scorefrom, scoreto, howmuch, clientid, takerid, template) VALUES (@scorefrom, @scoreto, @howmuch, @clientid, @takerid, @template);");
                        return Ok("Moneys has been sent");
                    }
                    return BadRequest("Check acceptor name!");  
                }
                return BadRequest("No money");
            }
            return BadRequest("No claim");
        }
    }
}