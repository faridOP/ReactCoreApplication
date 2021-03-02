using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Activities.DTOs;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class ListUserActivities
    {

        public class Query : IRequest<Result<List<UserActivityDto>>>
        {
            public string Username { get; set; }
            public string Predicate { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<UserActivityDto>>>
        {
            private readonly DataContext context;

            public Handler(DataContext context)
            {
                this.context = context;
            }

            public async Task<Result<List<UserActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {

                var query = context.ActivityAttendees
                .Where(x => x.AppUser.UserName == request.Username)
                .OrderBy(x => x.Activity.Date)
                .Select(aa => new UserActivityDto
                {
                    Category = aa.Activity.Category,
                    CreatedAt = aa.Activity.Date,
                    HostUsername = aa.Activity.Attendees.FirstOrDefault(x => x.IsHost).AppUser.UserName,
                    Title = aa.Activity.Title,
                    Id = aa.Activity.Id
                })
                .AsQueryable();

                switch (request.Predicate)
                {
                    case "past":
                        query = query.Where(x => x.CreatedAt <= DateTime.Now);
                        break;
                    case "hosting":
                        query = query.Where(x => x.HostUsername == request.Username);
                        break;
                    default:
                        query = query.Where(x => x.CreatedAt >= DateTime.Now);
                        break;
                }

                var activities = await query.ToListAsync();

                return Result<List<UserActivityDto>>.Success(activities);
            }
        }
    }

}