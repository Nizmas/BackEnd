using System;
using Npgsql;
using Dapper;

namespace TableSettings
{

    public class ClientService
    {
        public ClientModel ReturnClient(ClientModel client, string sqlCommand) 
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                Console.WriteLine("Connected to Getting");
                var result = conn.QuerySingleOrDefault<ClientModel>(sqlCommand,
                    new
                    {
                        userguid = client.UserGuid,
                        clientname=client.ClientName
                    });
                Console.WriteLine("Clients Data has been changed");
                return result;
            }
        }
        
        public void WorkClient(ClientModel client, string sqlCommand)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                conn.Execute(sqlCommand,
                    new
                    {
                        clientname = client.ClientName,
                        passwordhash = client.PasswordHash,
                        passwordsalt = client.PasswordSalt,
                        userguid = client.UserGuid,
                        realname = client.RealName
                    });
                Console.WriteLine("Clients data inserted");
            }
        }
        
        private NpgsqlConnection CreateConnection() 
        {
            var connection = new NpgsqlConnection($"server=localhost;database=bank;userid=postgres;password=admin;Pooling=false");

            return connection;
        }
    }
}