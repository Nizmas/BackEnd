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
    public class ShowTemplateController : ControllerBase
    {
        /// <summary>
        /// Контроллер для вывода шаблонов по конкретно заданному счёту
        /// работает и с чужим номером счёта, если оттуда производился перевод к владельцу
        /// </summary>
        /// <returns></returns>
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] TemplateCredentials templ)
        {
            var idClaim = User.Claims.FirstOrDefault(x =>
                x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var templService = new TemplateService();
                var scService = new ScoreService();
                string templateList = "";
                var workerCount = 0;
                var oldCount = 1;
                while (workerCount != oldCount)
                {
                    try
                    {
                        var showTemplate = templService.ReturnTemplate(new TemplateModel {ScoreFrom = templ.ScoreFrom, Id = workerCount, ClientId = tokenGuid},
                            "SELECT * FROM templates WHERE scorefrom = @scorefrom AND clientid = @clientid AND id >= @id;");
                        oldCount = workerCount;
                        workerCount = showTemplate.Id + 1;
                         if (scService.ReturnScore(
                                         new ScoreModel {ClientId = tokenGuid, NumScore = showTemplate.ScoreTo},
                                         "SELECT * FROM scores WHERE numscore=@numscore AND clientid=@clientid") != null && scService.ReturnScore(
                                         new ScoreModel {ClientId = tokenGuid, NumScore = showTemplate.ScoreFrom},
                                         "SELECT * FROM scores WHERE numscore=@numscore AND clientid=@clientid") != null)
                             templateList = templateList + "Transfer to: ";
                         else {templateList = templateList + "Payment for ";
                             templateList = templateList + showTemplate.TakerName + ": ";
                         }
                         templateList = templateList + showTemplate.ScoreTo + "\r\n";
                         templateList = templateList + "   " + showTemplate.HowMuch + " rub" + "\r\n\r\n";
                    }
                    catch
                    {
                        workerCount = oldCount;
                        if (templateList.Length > 4) templateList = templateList.Substring(0, templateList.Length - 4);
                    }
                }
                return Ok(templateList);
            }
            return BadRequest("No claim");
        }
    }
}