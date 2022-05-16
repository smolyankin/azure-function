using AzureFunction.Domain.Models;
using AzureFunction.Service.Requests;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AzureFunction.Service.Impl.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private const string TableName = "msdyn_timeentry";

        public async Task AddRange(TimeEntryDto timeEntry)
        {
            if (timeEntry is null || timeEntry.StartOn is null || timeEntry.EndOn is null)
                throw new ArgumentNullException(nameof(timeEntry));
            
            var existTimeEntries = GetList(timeEntry.StartOn.Value, timeEntry.EndOn.Value);

            using var serviceClient = new ServiceClient(Environment.GetEnvironmentVariable("DefaultConnection"));
            
            for (var start = timeEntry.StartOn.Value.Date; start <= timeEntry.EndOn.Value.Date; start = start.AddDays(1))
            {
                if (existTimeEntries.Any(x => x.Start.Day == start.Day))
                    continue;

                await CreateAsync(serviceClient, start);
            }
        }

        private static async Task CreateAsync(ServiceClient serviceClient, DateTime start)
        {
            var entity = new Entity(TableName);
            entity.Attributes = new AttributeCollection
                {
                    { "msdyn_date", start },
                    { "msdyn_start", start },
                    { "msdyn_end", start },
                    { "msdyn_duration", 0 },
                    { "msdyn_type", 192350000 },
                    { "statecode", 0 }
                };

            await serviceClient.CreateAsync(entity);
        }

        private IEnumerable<TimeEntry> GetList(DateTime start, DateTime end)
        {
            using var serviceClient = new ServiceClient(Environment.GetEnvironmentVariable("DefaultConnection"));
            
            var query = new QueryExpression(TableName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            query.Criteria.AddCondition("msdyn_date", ConditionOperator.GreaterEqual, start.Date);
            query.Criteria.AddCondition("msdyn_date", ConditionOperator.LessEqual, end.Date);

            var existTimeEntries = serviceClient.RetrieveMultiple(query).Entities;

            return existTimeEntries.Select(x => new TimeEntry
            {
                Start = x.GetAttributeValue<DateTime>("msdyn_start"),
                End = x.GetAttributeValue<DateTime>("msdyn_end")
            });
        }
    }
}
