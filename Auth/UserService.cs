using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TableSettings;

namespace Auth
{
    /// <summary>
    /// Класс для проверки наличия клиента в базе для авторизации или регистрации.
    /// CheckClient проверяет на наличие клиента с заданным именем
    /// IsActiveUser проверяет активность юзера (провёл ли его модератор)
    /// IsValidUser проверяет пароль
    /// </summary>
    public interface IUserService
    {
        bool IsValidUser (string username, string password);
        bool UserCreate (string username, string password, string name);
        bool IsActiveUser (string username); 
    } 
    
    public class UserService : IUserService
    {
        public bool IsValidUser(string username, string password)
        {
            var clService = new ClientService();
            if (!CheckClient(clService, username)) 
            {
                Console.WriteLine("Client is coming ");
                var selectedClient = clService.ReturnClient(new ClientModel{ClientName = username}, "SELECT * FROM clients WHERE clientname = @clientname;");
                return Password.CheckPassword(password, selectedClient.PasswordSalt, selectedClient.PasswordHash);
            }
            return false;
        }
        
        public bool IsActiveUser(string username) //проверить активность юзера
        {
            var clService = new ClientService();
            var selectedClient = clService.ReturnClient(new ClientModel{ClientName = username}, "SELECT * FROM clients WHERE clientname = @clientname;");
            return (selectedClient.ClientStatus);
        }
        
        public bool UserCreate(string username, string password, string name)
        {   
            var clService = new ClientService(); 
            if (CheckClient(clService, username)) 
            {
                Guid userGuid = Guid.NewGuid();
                Console.WriteLine("Making Client");
                var newUser = new User(username, password); 
                CreateClient(clService, newUser.Username, newUser.Password, newUser.ClientSalt, userGuid, name); 
                return true;
            }
            return false;
        }

        static bool CheckClient(ClientService clService, string username) 
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}"; 
            if (!Regex.Match(username.ToLower(), pattern, RegexOptions.IgnoreCase).Success) throw new ArgumentException("Insert E-mail addres");

            var expectedClient = clService.ReturnClient(new ClientModel{ClientName = username}, "SELECT * FROM clients WHERE clientname = @clientname;");
            return (expectedClient==null); 
        } 
        static void CreateClient(ClientService clService, string username, string password, string clientsalt, Guid id, string realname)
        {
            clService.WorkClient(new ClientModel{ClientName = username, PasswordHash = password, PasswordSalt = clientsalt, UserGuid = id, RealName = realname}, "INSERT INTO clients (clientname, passwordhash, passwordsalt, userguid, realname) VALUES (@clientname, @passwordhash, @passwordsalt, @userguid, @realname);");
        }
    }
}