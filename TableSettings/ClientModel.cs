using System;

namespace TableSettings
{
    public class ClientModel
    {
        public bool ClientStatus { get; set; }
        public string ClientName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }    
        public Guid UserGuid { get; set; }
        public string RealName { get; set; } 
    }
}