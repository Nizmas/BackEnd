using System;
using System.Linq;
using Dapper;
using Npgsql;

namespace TableSettings
{
    public class ScoreService
    {
        public ScoreModel ReturnScore(ScoreModel score, string sqlCommand) 
        { 
            using (var conn = CreateConnect())
            {
                conn.Open();
                var result = conn.QueryFirstOrDefault<ScoreModel>(sqlCommand,
                    new
                    {
                        id = score.Id,
                        clientid = score.ClientId,
                        numscore = score.NumScore,
                        cash = score.Cash,
                        exist = score.Exist
                    });
                Console.WriteLine("Returned Score");
                return result;
            }
        }
        
        public void WorkScore(ScoreModel score, string sqlCommand) 
        { 
            using (var conn = CreateConnect())
            {
                conn.Open();
                conn.Execute(sqlCommand,
                    new
                    {
                        clientid = score.ClientId,
                        numscore = score.NumScore,
                        cash = score.Cash,
                        exist = score.Exist
                    });
                Console.WriteLine("Worked Score");
            }
        }
        
        private NpgsqlConnection CreateConnect()
        {
            var connection = new NpgsqlConnection($"server=localhost;database=bank;userid=postgres;password=admin;Pooling=false");

            return connection;
        }
    }
}