using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MoneyService1.DTO;
using TableSettings;

namespace MoneyService1.Controllers
{
    [ApiController]
    [Authorize]
    public class ExtractController : ControllerBase
    {
        /// <summary>
        /// Контроллер для вывода Выписки по конкретно заданному счёту
        /// работает и с чужим номером счёта, если оттуда производился перевод к владельцу
        /// </summary>
        /// <returns></returns>
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] ScoreCredentials extractor)
        {
            var idClaim = User.Claims.FirstOrDefault(x =>
                x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var histService = new HistoryService();
                var scService = new ScoreService();
                var clService = new ClientService();
                var workerCount = 0;
                var oldCount = 1;
                string clientHistories = "Client: " + clService.ReturnClient(new ClientModel {UserGuid = tokenGuid}, "SELECT * FROM clients WHERE userguid = @userguid;").RealName + "\r\n";
                clientHistories = clientHistories + clService.ReturnClient(new ClientModel {UserGuid = tokenGuid}, "SELECT * FROM clients WHERE userguid = @userguid;").ClientName + "\r\n";
                clientHistories = clientHistories + "Detalization of: #"+ extractor.ScoreFrom + "\r\n\r\n";
                while (workerCount != oldCount)
                {
                    try
                    {
                        var showHistory = histService.ReturnHistory(new HistoryModel {ScoreFrom = extractor.ScoreFrom, Id = workerCount, ClientId = tokenGuid},
                            "SELECT * FROM viewhistories WHERE (scorefrom = @scorefrom AND clientid = @clientid AND id >= @id) OR (scoreto = @scorefrom AND takerid = @clientid AND id >= @id);");
                        oldCount = workerCount;
                        workerCount = showHistory.Id + 1;
                        if (showHistory.ScoreFrom.Equals("4000000000"))
                        {
                            clientHistories = clientHistories + "Refill ";
                            showHistory.ScoreFrom = "Deposit";
                        } else if (scService.ReturnScore(
                                       new ScoreModel {ClientId = tokenGuid, NumScore = showHistory.ScoreTo},
                                       "SELECT * FROM scores WHERE numscore=@numscore AND clientid=@clientid") != null && scService.ReturnScore(
                                       new ScoreModel {ClientId = tokenGuid, NumScore = showHistory.ScoreFrom},
                                       "SELECT * FROM scores WHERE numscore=@numscore AND clientid=@clientid") != null)
                                clientHistories = clientHistories + "Transfer ";
                            else clientHistories = clientHistories + "Payment ";
                            if (showHistory.Template) clientHistories = clientHistories + "by template ";
                            clientHistories = clientHistories + showHistory.SentTime + "\r\nFrom: " + showHistory.ScoreFrom;
                            clientHistories = clientHistories + "   To:" + showHistory.ScoreTo + "\r\n";
                            clientHistories = clientHistories + "   " + showHistory.HowMuch + " rub" + "\r\n\r\n";
                    }
                    catch
                    {
                        workerCount = oldCount;
                        if (clientHistories.Length > 4) clientHistories = clientHistories.Substring(0, clientHistories.Length - 4);
                    }
                }
                return Ok(clientHistories);
            }
            return BadRequest("No claim");
        }
    }
}