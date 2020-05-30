using System;
using Dapper;
using Npgsql;

namespace TableSettings
{
    public class HistoryService
    {
        /// <summary>
        /// Клласс для записи и считывания истории платежей и переводов
        /// </summary>
        /// <param name="history"> применяемые из таблицы данные</param>
        /// <param name="sqlCommand"> команды заброса к БД</param>
        /// <returns></returns>
        public HistoryModel ReturnHistory(HistoryModel history, string sqlCommand) 
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                Console.WriteLine("Connected to Messages");
                var result = conn.QueryFirstOrDefault<HistoryModel>(sqlCommand,
                    new
                    {
                        id = history.Id,
                        scorefrom = history.ScoreFrom,
                        scoreto = history.ScoreTo,
                        senttime = history.SentTime,
                        howmuch = history.HowMuch,
                        template = history.Template,
                        clientid = history.ClientId,
                        takerid = history.TakerId
                    });
                Console.WriteLine("History returned");
                return result;
            }
        }
        
        public void WorkHistory(HistoryModel history, string sqlCommand)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                conn.Execute(sqlCommand,
                    new
                    {
                        id = history.Id,
                        scorefrom = history.ScoreFrom,
                        scoreto = history.ScoreTo,
                        senttime = history.SentTime,
                        howmuch = history.HowMuch,
                        template = history.Template,
                        clientid = history.ClientId,
                        takerid = history.TakerId
                    });
                Console.WriteLine("History worked");
            }
        }
        
        private NpgsqlConnection CreateConnection() 
        {
            var connection = new NpgsqlConnection($"server=localhost;database=bank;userid=postgres;password=admin;Pooling=false");

            return connection;
        }
    }
}