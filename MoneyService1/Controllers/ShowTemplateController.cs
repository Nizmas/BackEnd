using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
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
        [HttpGet]
        public ActionResult<string> ShowTempl()
        {
            var idClaim = User.Claims.FirstOrDefault(x =>
                x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var templService = new TemplateService();
                var scService = new ScoreService();
                var workerCount = 0;
                var oldCount = 1;
                List<Templates> tempList = new List<Templates>();
                while (workerCount != oldCount)
                {
                    try
                    {
                        var showTemplate = templService.ReturnTemplate(new TemplateModel {Id = workerCount, ClientId = tokenGuid},
                            "SELECT * FROM templates WHERE clientid = @clientid AND id >= @id;");
                        oldCount = workerCount;
                        workerCount = showTemplate.Id + 1;
                        
                        Templates tmp = new Templates();
                        tmp.TemplateName = showTemplate.TemplateName;
                        tmp.ScoreFrom = showTemplate.ScoreFrom;
                        tmp.ScoreTo = showTemplate.ScoreTo;
                        tmp.HowMuch = showTemplate.HowMuch.ToString();
                        tmp.TakerName = showTemplate.TakerName;
                        tempList.Add(tmp);
                    }
                    catch
                    {
                        workerCount = oldCount;
                    }
                }
                return Ok(tempList);
            }
            return BadRequest("No claim");
        }
    }
    
    class Templates
    {
        public string TemplateName { get; set; }
        public string ScoreFrom { get; set; }
        public string ScoreTo { get; set; }
        public string HowMuch { get; set; }
        public string TakerName { get; set; }
    }
}