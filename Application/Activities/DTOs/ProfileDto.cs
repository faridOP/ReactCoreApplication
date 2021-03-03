using System.Collections.Generic;
using Domain;

namespace Application.DTOs
{
    public class ProfileDto
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string Image {get;set;}
        public ICollection<Photo> Photos {get;set;}
        public bool isFollowing {get;set;}
        public int FollowersCount {get;set;}
        public int FollowingsCount{get;set;}
    }
}