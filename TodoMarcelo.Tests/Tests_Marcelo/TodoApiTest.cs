using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TodoMarcelo.Common.Models;
using TodoMarcelo.Functions.Functions;
using TodoMarcelo.Tests.Helpers;
using Xunit;

namespace TodoMarcelo.Tests.Tests_Marcelo
{
    public class TodoApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        //PRUEBA UNITARIA PARA EL CREATETODO
        [Fact]
        public async void CreateTodo_Should_Return_200()
        {
            // Arrage (preparar prueba unitaria)
            MockCloudTableTodos mockTodos = new MockCloudTableTodos(new Uri("htpp://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo todoRequest = TestFactory.GetTodoRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(todoRequest);

            // Act (ejecutar prueba unitaria)
            IActionResult response = await TodoApi.CreateTodo(request, mockTodos, logger);

            // Assert (verificar si la prueba unitario dio el resultado correcto)
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


        [Fact]
        public async void UpdateTodo_Should_Return_200()
        {
            // Arrage (preparar prueba unitaria)
            MockCloudTableTodos mockTodos = new MockCloudTableTodos(new Uri("htpp://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid todoId = Guid.NewGuid();
            Todo todoRequest = TestFactory.GetTodoRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(todoId,todoRequest);

            // Act (ejecutar prueba unitaria)
            IActionResult response = await TodoApi.UpdateTodo(request,mockTodos,todoId.ToString(),logger);

            // Assert (verificar si la prueba unitario dio el resultado correcto)
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

    }
}
