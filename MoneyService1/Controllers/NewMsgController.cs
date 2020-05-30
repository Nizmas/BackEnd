using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyService1.DTO;
using TableSettings;

namespace MoneyService1.Controllers
{
    /// <summary>
    /// Контроллер для обработки чата со службой поддержки
    /// </summary>
    [ApiController]
    [Authorize]
    public class NewMsgController : ControllerBase
    {
        [Route("[controller]")]
        [HttpPost]
        public IActionResult Get([FromBody] MessageCredentials newMessage)
        {
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Jti, StringComparison.InvariantCultureIgnoreCase));

            if (idClaim != null)
            {
                Guid tokenGuid = Guid.Parse(idClaim.ToString().Remove(0, 5));
                var msgService = new MessageService();
                msgService.WorkMessage(new MessageModel {ClientId = tokenGuid, AuthorId = tokenGuid, Message = newMessage.NewMessage}, "INSERT INTO messages (clientid, authorid, message) VALUES (@clientid, @authorid, @message);");
                var workerCount = 0;
                var oldCount = 1;
                List<Messages> msgList = new List<Messages>();
                while (workerCount != oldCount)
                {
                    try
                    {
                        var showMessage = msgService.ReturnMessage(new MessageModel {ClientId = tokenGuid, Id = workerCount},
                            "SELECT * FROM viewmessages WHERE clientid = @clientid AND id >= @id;");
                        oldCount = workerCount;
                        workerCount = showMessage.Id + 1;
                        Messages msg = new Messages();
                        if (showMessage.AuthorId == tokenGuid) msg.Author = "Вы";
                        else msg.Author = "Сотрудник";
                        msg.Msg = showMessage.Message; msg.SentTime = showMessage.SentTime;
                        msgList.Add(msg);
                    }
                    catch
                    {
                        workerCount = oldCount;
                    }
                }
                return Ok(msgList);
            }
            return BadRequest("Try to enter one more time");
        }
        
    }
}