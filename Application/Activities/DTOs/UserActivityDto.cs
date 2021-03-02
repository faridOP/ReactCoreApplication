using System;
using System.Text.Json.Serialization;

namespace Application.Activities.DTOs
{
    public class UserActivityDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        
        [JsonIgnore]
        public string  HostUsername { get; set; }
    }
}