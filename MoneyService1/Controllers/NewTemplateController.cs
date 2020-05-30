using System;
using System.Linq;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MoneyService1.DTO;
using TableSettings;

namespace MoneyService1.Controllers
{
    [ApiController]
    [Authorize]
    public class NewTemplateController : ControllerBase
    {
        /// <summary>
        /// Контроллер для создания шаблонов. Все данные берутся из json кроме GuidId
        /// </summary>
        /// <returns>сообщение об успешности выполнения</returns>
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] TemplateCredentials templ)
        {
            var idClaim = User.Claims.FirstOrDefault(x =>
                x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var temService = new TemplateService();
                var clService = new ClientService();
                var scService = new ScoreService();
                try
                {
                    var takerId = clService.ReturnClient(new ClientModel {ClientName = templ.TakerName},
                        "SELECT * FROM clients WHERE clientname = @clientname");
                    if (scService.ReturnScore(new ScoreModel {NumScore = templ.ScoreTo, ClientId = takerId.UserGuid},
                        "SELECT * FROM scores WHERE clientid = @clientid AND numscore = @numscore AND exist = TRUE;") != null) // проверить, принадлежит ли номер счёта принимателю
                    {
                        temService.WorkTemplate(
                            new TemplateModel
                            {
                                TemplateName = templ.TemplateName, ClientId = tokenGuid, HowMuch = templ.HowMuch,
                                ScoreFrom = templ.ScoreFrom, ScoreTo = templ.ScoreTo, TakerName = templ.TakerName
                            },
                            "INSERT INTO templates (templatename, scorefrom, scoreto, howmuch, clientid, takername) VALUES (@templatename, @scorefrom, @scoreto, @howmuch, @clientid, @takername)");
                        return Ok("Template is saved!");
                    }
                }
                catch
                {
                    return BadRequest("Check acceptor name!");
                }
                return BadRequest("No clients with this mail!");
            }
            return BadRequest("Enter one more time!");
        }
    }
}