using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TodoMarcelo.Functions.Functions;
using TodoMarcelo.Tests.Helpers;
using Xunit;

namespace TodoMarcelo.Tests.Tests_Marcelo
{
    public class ScheduledFunctionTest
    {
       
        [Fact]
        public void ScheduledFunction_Should_Log_Message()
        {
            // Arrage (preparar prueba unitaria)
            MockCloudTableTodos mockTodos = new MockCloudTableTodos(new Uri("htpp://127.0.0.1:10002/devstoreaccount1/reports"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act (ejecutar prueba unitaria)
            ScheduleFunction.Run(null, mockTodos, logger);
            string message = logger.Logs[0];

            // Assert (verificar si la prueba unitario dio el resultado correcto)
            Assert.Contains("Deleting completed",message);

        }


    }
}
