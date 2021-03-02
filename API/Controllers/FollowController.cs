using System.Threading.Tasks;
using Application.Followers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class FollowController : BaseApiController
    {
        [HttpPost("{username}")]
        public async Task<IActionResult> Follow(string username)
        {
            return HandleResult(await Mediator.Send(new FollowToggle.Command { TargetUsername = username }));
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetFollowing(string predicate, string username) //  [HttpGet("{username}")] should be equal to the parameter "string username" we receive, or it won't be recognized
        {
            return HandleResult(await Mediator.Send(new List.Query{Username=username,Predicate=predicate}));
        } 
    }
}