namespace Application.Activities.DTOs
{
    public class AttendeeDto
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public bool isFollowing {get;set;}
        public int FollowersCount {get;set;}
        public int FollowingsCount{get;set;}
        
    }
}