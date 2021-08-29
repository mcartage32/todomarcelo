using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using TodoMarcelo.Functions.Entities;

namespace TodoMarcelo.Functions.Functions
{
    public static class ScheduleFunction
    {
        [FunctionName("ScheduleFunction")]

        /*CREAMOS LA FUNCION CON UN TRIGGER DE TIEMPO Y LE ADICIONAMOS LA CONEXION A LA TABLA*/
        public static async Task Run(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
            ILogger log)
        {
            /*MANDAMOS EL MENSAJE Y */
            log.LogInformation($"Delenting completed function executed at: {DateTime.Now}");

            string filter = TableQuery.GenerateFilterConditionForBool("IsCompleted", QueryComparisons.Equal, true);
            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>().Where(filter);
            TableQuerySegment<TodoEntity> completedTodos = await todotable.ExecuteQuerySegmentedAsync(query, null);
            int deleted = 0;

            foreach (TodoEntity completedTodo in completedTodos)
            {
                await todotable.ExecuteAsync(TableOperation.Delete(completedTodo));
                deleted++;
            }

            log.LogInformation($"Deleted {deleted} items at: {DateTime.Now}");





        }
    }
}
