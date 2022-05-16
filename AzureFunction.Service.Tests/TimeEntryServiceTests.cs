using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AzureFunction.App.Functions;
using AzureFunction.Service.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace AzureFunction.Service.Tests
{
    public class TimeEntryServiceTests
    {
        private readonly Mock<ITimeEntryService> _timeEntryService;
        private readonly TimeEntryFunctions _timeEntryFunctions;

        public TimeEntryServiceTests()
        {
            _timeEntryService = new Mock<ITimeEntryService>();
            _timeEntryFunctions = new TimeEntryFunctions(_timeEntryService.Object);
        }

        [Theory]
        [InlineData("2022-01-01", "2022-01-01", "OkResult")]
        [InlineData("2022-01-01", null, "BadRequestObjectResult")]
        [InlineData(null, "2022-01-01", "BadRequestObjectResult")]
        [InlineData("2022-01-02", "2022-05-01", "OkResult")]
        public async Task Test1(string start, string end, string resultType)
        {
            //Arrange
            var request = GenerateHttpRequest(start, end);

            //Act
            var response = await _timeEntryFunctions.Add(request);

            //Assert
            var expectedType = Assembly.GetAssembly(typeof(OkResult))?.GetType($"Microsoft.AspNetCore.Mvc.{resultType}");
            Assert.IsType(expectedType, response);
        }

        private DefaultHttpRequest GenerateHttpRequest(string start, string end)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var timeEntry = new TimeEntryDto
            {
                StartOn = DateTime.TryParse(start, out DateTime startDate) ? startDate : null,
                EndOn = DateTime.TryParse(end, out DateTime endDate) ? endDate : null
            };
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(timeEntry)));
            request.Body = stream;
            request.ContentLength = stream.Length;

            return request;
        }
    }
}