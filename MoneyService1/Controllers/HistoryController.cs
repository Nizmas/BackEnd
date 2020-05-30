using System;
using System.Collections.Generic;
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
    public class HistoryController : ControllerBase
    {
        /// <summary>
        /// Контроллер для вывода списка с историей всех операций. В котором за пополнение принят перевод средств с буферного счёта 4000000000
        /// перевод - между своими счетами
        /// платёж - на чужой счёт
        /// </summary>
        /// <returns></returns>
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] HistoryCredentials operation)
        {
            var idClaim = User.Claims.FirstOrDefault(x =>
                x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var histService = new HistoryService();
                var scService = new ScoreService();
                var workerCount = 0;
                var oldCount = 1;
                List<Operations> historyList = new List<Operations>();
                while (workerCount != oldCount)
                {
                    try
                    {
                        var showHistory = histService.ReturnHistory(new HistoryModel {ClientId = tokenGuid, Id = workerCount, ScoreFrom = operation.ScoreFrom},
                            "SELECT * FROM viewhistories WHERE (clientid = @clientid AND scorefrom=@scorefrom AND id >= @id) OR (takerid = @clientid AND scoreto=@scorefrom AND id >= @id);");
                        oldCount = workerCount;
                        workerCount = showHistory.Id + 1;
                        if (showHistory.SentTime<=operation.TimeTo && showHistory.SentTime>=operation.TimeFrom)
                        {
                            Operations hist = new Operations();
                            if (showHistory.ScoreFrom == "4000000000")  hist.Type ="Пополнение";
                            else if (scService.ReturnScore(
                                         new ScoreModel {ClientId = tokenGuid, NumScore = showHistory.ScoreTo},
                                         "SELECT * FROM scores WHERE numscore=@numscore AND clientid=@clientid") !=
                                     null && scService.ReturnScore(
                                         new ScoreModel {ClientId = tokenGuid, NumScore = showHistory.ScoreFrom},
                                         "SELECT * FROM scores WHERE numscore=@numscore AND clientid=@clientid") !=
                                     null)
                                hist.Type ="Перевод";
                            else hist.Type = "Платёж";
                            
                            hist.ByTemplate = showHistory.Template;
                            hist.SentTime = showHistory.SentTime;
                            hist.ScoreFrom = showHistory.ScoreFrom;
                            hist.ScoreTo = showHistory.ScoreTo;
                            hist.HowMuch = showHistory.HowMuch;
                            historyList.Add(hist);
                        }
                    }
                    catch
                    {
                        workerCount = oldCount;
                    }
                }
                return Ok(historyList);
            }
            return BadRequest("No claim");
        }
    }
    class Operations
    {
        public string Type { get; set; }
        public bool ByTemplate { get; set; }
        public DateTime SentTime { get; set; }
        public string ScoreFrom { get; set; }
        public string ScoreTo { get; set; }
        public float HowMuch { get; set; }
    }
}