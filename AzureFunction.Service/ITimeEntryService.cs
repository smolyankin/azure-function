using AzureFunction.Service.Requests;

namespace AzureFunction.Service
{
    public interface ITimeEntryService
    {
        Task AddRange(TimeEntryDto timeEntry);
    }
}