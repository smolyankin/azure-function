using System;
using System.ComponentModel.DataAnnotations;

namespace AzureFunction.Service.Requests
{
    public class TimeEntryDto
    {
        [Required]
        public DateTime? StartOn { get; set; }

        [Required]
        public DateTime? EndOn { get; set; }
    }
}
