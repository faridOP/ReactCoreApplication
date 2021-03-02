using Microsoft.AspNetCore.Mvc;
using Domain;
using System.Threading.Tasks;
using Application.Activities;
using System;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Application.Core;

namespace API.Controllers
{
    // [AllowAnonymous]
    public class ActivitiesController : BaseApiController
    {
    
        // private readonly IMediator _mediator;
        // public ActivitiesController(IMediator mediator)
        // { 
        //     _mediator = mediator;
        // }

        [HttpGet]
        [Route("List")]
        [Route("")]
        // [Authorize(Roles="CustomFreddy")]
        public async Task<IActionResult> List([FromQuery] ActivityParams param)
        {
            return HandlePagedResult(await Mediator.Send(new List.Query{Params=param}));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Details(Guid id){


           return  HandleResult(await Mediator.Send(new Details.Query{Id=id}));

            // if(activity==null) return NotFound(); This is one way of 'normal' error handling.
        }
        [HttpPost]
        public async Task<IActionResult> Create(Activity activity){
            return HandleResult(await Mediator.Send(new Create.Command{Activity = activity}));
        }

        [Authorize(Policy="IsActivityHost")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid Id,Activity activity){
            activity.Id=Id;
            
            return HandleResult(await Mediator.Send(new Edit.Command{Activity=activity}));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid Id){
            
            return HandleResult(await Mediator.Send(new Delete.Command{Id=Id}));
        }

        [HttpPost("{id}/attend")]
        public async Task<IActionResult> Attend(Guid id){
            return HandleResult(await Mediator.Send(new UpdateAttendance.Command{Id=id}));
        }
    }


}