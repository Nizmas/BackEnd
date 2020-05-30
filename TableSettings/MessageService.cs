using System;
using Dapper;
using Npgsql;

namespace TableSettings
{
    public class MessageService
    {
        public MessageModel ReturnMessage(MessageModel message, string sqlCommand) 
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                Console.WriteLine("Connected to Messages");
                var result = conn.QueryFirstOrDefault<MessageModel>(sqlCommand,
                    new
                    {
                        id = message.Id,
                        clientid = message.ClientId,
                        authorid = message.AuthorId,
                        senttime = message.SentTime,
                        message = message.Message
                    });
                Console.WriteLine("MessageSent");
                return result;
            }
        }
        
        public void WorkMessage(MessageModel message, string sqlCommand)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                conn.Execute(sqlCommand,
                    new
                    {
                        id = message.Id,
                        clientid = message.ClientId,
                        authorid = message.AuthorId,
                        senttime = message.SentTime,
                        message = message.Message
                    });
                
            }
        }
        
        private NpgsqlConnection CreateConnection() 
        {
            var connection = new NpgsqlConnection($"server=localhost;database=bank;userid=postgres;password=admin;Pooling=false");

            return connection;
        }
    }
}