using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using AzureFunction.Service;
using AzureFunction.Service.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace AzureFunction.App.Functions
{
    public class TimeEntryFunctions
    {
        private readonly ITimeEntryService _timeEntryService;

        public TimeEntryFunctions(ITimeEntryService timeEntryService)
        {
            _timeEntryService = timeEntryService;
        }

        [FunctionName("TimeEntry_Add")]
        public async Task<IActionResult> Add(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)
        {
            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var timeEntryDto = JsonConvert.DeserializeObject<TimeEntryDto>(requestBody);

            var context = new ValidationContext(timeEntryDto, serviceProvider: null, items: null);
            var errorResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(timeEntryDto, context, errorResults, true))
                return new BadRequestObjectResult(timeEntryDto);

            await _timeEntryService.AddRange(timeEntryDto);

            return new OkResult();
        }
    }
}
