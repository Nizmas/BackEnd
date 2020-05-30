using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyService1.DTO;
using Auth;
using Microsoft.AspNetCore.Cors;

namespace MoneyService1.Controllers
{
    /// <summary>
    /// Данный контроллер написан для принятия данных нового пользователя: почтовый адрес, пароль и подтверждение пароля
    /// в функции Get из json принимаем данные и сравниваем принятый пароль с проверочным паролем и потом запускаем
    /// функцию UserCreate, который в последствии создаёт соль и хэширует пароль
    /// </summary>
    
    [ApiController]
    [EnableCors("MyPolicy")]
    public class NewUserController : ControllerBase
    {
        private IUserService _createService;
        public NewUserController(IUserService createService)
        {
            _createService = createService;
        }
       
        [Route("[controller]")]
        [HttpPost]
        [EnableCors("MyPolicy")]
        public IActionResult Get([FromBody] UserCredentials newUser)
        {
            if (Equals (newUser.Password,newUser.PasswordCheck))
            {
                if (newUser.RealName.Length<4) return Unauthorized("Name must to be longer");
                if (newUser.Password.Length<4) return Unauthorized("Password must to be longer");
                if (_createService.UserCreate(newUser.UserName, newUser.Password, newUser.RealName))
                {
                    return Ok("User is successfully created! Welcome!");
                }
                return Unauthorized("This user is exists");
            }
            return Unauthorized("Check your password!");
        }
    }
}