using System;

namespace Auth
{
    public class User
    {
        public User(string username, string password)
        {
            Username = username; // просто сохраняем имя пользователя
            _password = new Password (password); // запуск функции создания хэша пароля
            
        }
        
        private Password _password;
        public string Username { get; set; }
        public string Password => _password.PasswordHash;
        public string ClientSalt => _password.Salt;
    }
}