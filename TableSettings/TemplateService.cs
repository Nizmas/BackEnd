using System;
using Dapper;
using Npgsql;

namespace TableSettings
{
    public class TemplateService
    {/// <summary>
     /// Класс для работы с таблицей шаблонов платежей: выборка и изменение данных
     /// </summary>
     /// <param name="template">названия используемых столбцов таблиц</param>
     /// <param name="sqlCommand">строка с командой для базы данных</param>
     /// <returns></returns>
        public TemplateModel ReturnTemplate(TemplateModel template, string sqlCommand) 
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var result = conn.QueryFirstOrDefault<TemplateModel>(sqlCommand,
                    new
                    {
                        id = template.Id,
                        templatename = template.TemplateName,
                        clientid = template.ClientId,
                        scorefrom = template.ScoreFrom,
                        scoreto = template.ScoreTo,
                        howmuch = template.HowMuch,
                        takername = template.TakerName
                    });
                Console.WriteLine("Template Data has been changed");
                return result;
            }
        }
        
        public void WorkTemplate(TemplateModel template, string sqlCommand)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                conn.Execute(sqlCommand,
                    new
                    {
                        id = template.Id,
                        templatename = template.TemplateName,
                        clientid = template.ClientId,
                        scorefrom = template.ScoreFrom,
                        scoreto = template.ScoreTo,
                        howmuch = template.HowMuch,
                        takername = template.TakerName
                    });
                Console.WriteLine("Template data inserted");
            }
        }
        
        private NpgsqlConnection CreateConnection() 
        {
            var connection = new NpgsqlConnection($"server=localhost;database=bank;userid=postgres;password=admin;Pooling=false");

            return connection;
        }
    }
}