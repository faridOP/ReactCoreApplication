using Application.Core;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities
{
    public class List
    {
        //We define our request query here, and type of its response, in this case, Activity
        public class Query : IRequest<Result<PagedList<ActivityDto>>> {

            public ActivityParams Params { get; set; }
         }

        //Request is Query and Activity is response, respectively
        public class Handler : IRequestHandler<Query, Result<PagedList<ActivityDto>>>
        {
            private readonly DataContext _context;
         
            private readonly IMapper _mapper;
            private readonly IUserAccessor userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
            
                _mapper = mapper;
                this.userAccessor = userAccessor;
            }

            public async Task<Result<PagedList<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {


                //More efficient way, mapping through the properties only we require

                var query = _context.Activities
                .Where(x=>x.Date>=request.Params.StartDate)
                .OrderBy(d=>d.Date)
                .Select(activity=> new ActivityDto {
                    Id = activity.Id,
                    City=activity.City,
                    Date=activity.Date,
                    Description=activity.Description,
                    Title=activity.Title,
                    Venue=activity.Venue,
                    Category = activity.Category,
                    HostUsername=activity.Attendees.FirstOrDefault(x=>x.IsHost).AppUser.UserName,
                    Attendees = activity.Attendees.Select(at=>new DTOs.AttendeeDto{
                        Username = at.AppUser.UserName,
                        DisplayName=at.AppUser.DisplayName,
                        Bio=at.AppUser.Bio,
                        Image = at.AppUser.Photos.FirstOrDefault(x=>x.IsMain).Url,
                        FollowersCount=at.AppUser.Followers.Count,
                        FollowingsCount=at.AppUser.Followings.Count,
                        isFollowing=at.AppUser.Followers.Any(x=>x.Observer.UserName==userAccessor.GetUserName())
                    })
                })
                .AsQueryable();

                if(request.Params.IsGoing&&!request.Params.IsHost){
                    query = query.Where(x=>x.Attendees.Any(x=>x.Username==userAccessor.GetUserName()));
                }

                if(request.Params.IsHost && !request.Params.IsGoing)
                {
                    query= query.Where(x=>x.HostUsername==userAccessor.GetUserName());
                }
                return Result<PagedList<ActivityDto>>.Success(

                    await PagedList<ActivityDto>.CreateAsync(query, request.Params.PageNumber,request.Params.PageSize)
                );
            } 
        }
    }
}


                //Mapping using IMapper by processing through all data (eg AppUser's LockoutEnabled or other unused properties), which is inefficient a little bit

                // var activites = await _context.Activities
                // .Include(a=>a.Attendees)
                // .ThenInclude(u=>u.AppUser)
                // .ToListAsync(cancellationToken);
    
                // var activitiesToReturn = _mapper.Map<List<ActivityDto>>(activites);

                // return Result<List<ActivityDto>>.Success(activitiesToReturn);


                //Mapping in a traditional way, more code

                // var activites = await _context.Activities
                // .Select(activity=> new ActivityDto {
                //     Id = activity.Id,
                //     City=activity.City,
                //     Date=activity.Date,
                //     Description=activity.Description,
                //     Title=activity.Title,
                //     Venue=activity.Venue,
                //     Category = activity.Category,
                //     HostUsername=activity.Attendees.FirstOrDefault(x=>x.IsHost).AppUser.UserName,
                //     Attendees = activity.Attendees.Select(at=>new DTOs.AttendeeDto{
                //         Username = at.AppUser.UserName,
                //         DisplayName=at.AppUser.DisplayName,
                //         Bio=at.AppUser.Bio,
                //         Image = at.AppUser.Photos.FirstOrDefault(x=>x.IsMain).Url   ,
                //         FollowersCount=at.AppUser.Followers.Count,
                //         FollowingsCount=at.AppUser.Followings.Count,
                //         isFollowing=at.AppUser.Followers.Any(x=>x.Observer.UserName==userAccessor.GetUserName())
                //     })
                // })
                // .ToListAsync(cancellationToken);

                
                //More efficient way, mapping through the properties only we require

                // var activites = await _context.Activities
                // .AsNoTracking()
                // .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider, new{currentUsername = userAccessor.GetUserName()})
                // .ToListAsync(cancellationToken);