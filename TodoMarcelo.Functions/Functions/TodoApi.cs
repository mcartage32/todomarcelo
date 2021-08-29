using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TodoMarcelo.Common.Models;
using TodoMarcelo.Common.Responses;
using TodoMarcelo.Functions.Entities;

namespace TodoMarcelo.Functions.Functions
{
    public static class TodoApi
    {
        /*------------------------------CREACION DE LA FUNCION DE INSERCION O CREACION--------------------*/
        [FunctionName(nameof(CreateTodo))]
        /* ACA INYECTAMOS A LA FUNCION UN HTTP TRIGGER Y UNA TABLA DE LA CLASE CLOUDTABLE*/
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
            ILogger log)
        {
            //LE MANDAMOS UN MENSAJE AL LOG Y LEEMOS EL BODY
            log.LogInformation("Recieved a new todo.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // ACA CREAMOS UN OBJETO todo QUE DESERIALIZA EL JSON DE LO QUE LEYO EL BODY
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // ACA VERIFICAMOS SI EL OBJETO todo ES NULLO Y/0 SU TASKDESCRIPTION ES NULO
            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSucess = false,
                    Message = "The request must have a TaskDescription."

                });

            }

            // SI NO ES NULO CREAMOS EL OBJETO todoEntity 
            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription,
            };

            //DEL NUGGET LLAMAMOS LA CLASE TableOperation PARA INSERTAR EL OBJETO DE LA ENTIDAD
            TableOperation addOperation = TableOperation.Insert(todoEntity);
            // EJECUTAMOS LA OPERACION
            await todotable.ExecuteAsync(addOperation);

            //SI TODO FUNCIONO CORRECTAMENTE CREAMOS UN MENSAJE DE BIEN HECHO Y LO CARGAMOS AL LOG (CONSOLA)
            string message = "New todo stored in table.";
            log.LogInformation(message);


            // RETORNAMOS UNA RESPUESTA POSITIVA CON QUE SI SE CUMPLIO LA EJECUCION, CON EL MENSAJE Y CON LA ENTIDAD CREADA
            return new OkObjectResult(new Responses
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity,


            });
        }


        /*-------------------CREACION DE LA FUNCION DE UPDATE-----------------------------------*/

        [FunctionName(nameof(UpdateTodo))]
        /* ACA INYECTAMOS A LA FUNCION UN HTTP TRIGGER Y UNA TABLA DE LA CLASE CLOUDTABLE*/
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for todo {id} recieved.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            //VALIDAR EL ID DEL todo
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todotable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSucess = false,
                    Message = "Todo not found."

                });

            }

            //Validacion del campo taskdescription y actualizamos la descripcion y actualizamos a que si se hizo en la propiedad completado

            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.IsCompleted = todo.IsCompleted;

            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;

            }

            //DEL NUGGET LLAMAMOS LA CLASE TableOperation PARA INSERTAR EL OBJETO DE LA ENTIDAD
            TableOperation addOperation = TableOperation.Replace(todoEntity);
            // EJECUTAMOS LA OPERACION
            await todotable.ExecuteAsync(addOperation);

            //SI TODO FUNCIONO CORRECTAMENTE CREAMOS UN MENSAJE DE BIEN HECHO Y LO CARGAMOS AL LOG (CONSOLA)
            string message = $"Todo {id}, update in table.";
            log.LogInformation(message);


            // RETORNAMOS UNA RESPUESTA POSITIVA CON QUE SI SE CUMPLIO LA EJECUCION, CON EL MENSAJE Y CON LA ENTIDAD CREADA
            return new OkObjectResult(new Responses
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity,


            });
        }

        /*-------------------CREACION DE LA FUNCION TRAER TODOS LOS DATOS DE LA "TABLA"-----------------*/

        [FunctionName(nameof(GetAllTodos))]
        /* ACA INYECTAMOS A LA FUNCION UN HTTP TRIGGER Y UNA TABLA DE LA CLASE CLOUDTABLE*/
        public static async Task<IActionResult> GetAllTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
            ILogger log)
        {
            //LE MANDAMOS UN MENSAJE AL LOG Y CREAMOS EL QUERY POR MEDIO DE UNA TABLA Y LA FILTRAMOS CON LA OTRA TABLA
            log.LogInformation("Get all todos received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todotable.ExecuteQuerySegmentedAsync(query, null);


            //SI TODO FUNCIONO CORRECTAMENTE CREAMOS UN MENSAJE DE BIEN HECHO Y LO CARGAMOS AL LOG (CONSOLA)
            string message = "Retrieved all todos.";
            log.LogInformation(message);


            // RETORNAMOS UNA RESPUESTA POSITIVA CON QUE SI SE CUMPLIO LA EJECUCION, CON EL MENSAJE Y CON LA ENTIDAD CREADA
            return new OkObjectResult(new Responses
            {
                IsSucess = true,
                Message = message,
                Result = todos,


            });
        }


        /*-------------------CREACION DE LA FUNCION TRAER UN SOLO REGISTRO DE LA "TABLA" -----------------*/

        [FunctionName(nameof(GetTodoById))]
        /* ACA INYECTAMOS A LA FUNCION UN HTTP TRIGGER Y UN OBJETO DE LA CLASE TodoEntity que ya trae el id que buscamos*/
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
            string id,
            ILogger log)
        {
            //LE MANDAMOS UN MENSAJE AL LOG Y VALIDAMOS SI EL OBJETO todoEntity NO LLEGO NULO
            log.LogInformation($"Get todo by id:{id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSucess = false,
                    Message = "Todo not found."

                });

            }


            //SI TODO FUNCIONO CORRECTAMENTE CREAMOS UN MENSAJE DE BIEN HECHO Y LO CARGAMOS AL LOG (CONSOLA)
            string message = $"Todo {todoEntity.RowKey} retrieved.";
            log.LogInformation(message);


            // RETORNAMOS UNA RESPUESTA POSITIVA CON QUE SI SE CUMPLIO LA EJECUCION, CON EL MENSAJE Y CON LA ENTIDAD CREADA
            return new OkObjectResult(new Responses
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity,


            });
        }




        /*-------------------CREACION DE LA FUNCION BORRAR -----------------*/

        [FunctionName(nameof(DeleteTodo))]
/* ACA INYECTAMOS A LA FUNCION UN HTTP TRIGGER Y UN OBJETO DE LA CLASE TodoEntity que ya trae el id que buscamos y una tabla con la conexion*/
        public static async Task <IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
            string id,
            ILogger log)
        {
            //LE MANDAMOS UN MENSAJE AL LOG Y VALIDAMOS SI EL OBJETO todoEntity NO LLEGO NULO
            log.LogInformation($"Delete todo :{id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    IsSucess = false,
                    Message = "Todo not found."

                });

            }


            //SI TODO FUNCIONO CORRECTAMENTE CREAMOS UN MENSAJE DE BIEN HECHo. EJECUTAMOS EN UN SOLA LINEA EL BORRAR REGISTRO
            await todotable.ExecuteAsync(TableOperation.Delete(todoEntity));
            string message = $"Todo {todoEntity.RowKey} deleted.";
            log.LogInformation(message);


            // RETORNAMOS UNA RESPUESTA POSITIVA CON QUE SI SE CUMPLIO LA EJECUCION, CON EL MENSAJE Y CON LA ENTIDAD CREADA
            return new OkObjectResult(new Responses
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity,


            });
        }




    }
}
