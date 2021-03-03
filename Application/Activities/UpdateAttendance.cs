using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities
{
    public class UpdateAttendance
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }


        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _useraccessor;
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _useraccessor = userAccessor;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities
             .Include(a => a.Attendees).ThenInclude(a => a.AppUser)
             .FirstOrDefaultAsync(a => a.Id == request.Id);

                if (activity == null) return null;

                var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _useraccessor.GetUserName()
                );

                if (user == null) return null;

                string hostUsername = activity.Attendees.FirstOrDefault(x => x.IsHost)?.AppUser?.UserName;

                ActivityAttendee attendance = activity.Attendees.FirstOrDefault(c => c.AppUser.UserName == user.UserName);

                if (attendance != null)
                {
                    if (hostUsername == user.UserName)
                    {
                        activity.IsCancelled = !activity.IsCancelled;

                    }

                    if (hostUsername != user.UserName)
                    {
                        activity.Attendees.Remove(attendance);
                    }
                }
                else
                {
                    attendance = new ActivityAttendee
                    {
                        AppUser = user,
                        Activity = activity,
                        IsHost = false
                    };
                    activity.Attendees.Add(attendance);
                }


                bool result = await _context.SaveChangesAsync() > 0;

                if (!result)
                {
                    Result<Unit>.Failure("Problem updating attendance");
                }

                return Result<Unit>.Success(Unit.Value);

            }


        }
    }
}


//SingleOrDefaultAsync returns exception if there are more than one item with same id, but FirstOrDefaultAsync does not, it only takes the first one that matches.
