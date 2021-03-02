using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>> //Previosly : public class Command : IRequest , we do not return anything from command, and we specify Unit in newer version just for informing MediatR that we do not return any value but status.
        {
            public Activity Activity { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Activity).SetValidator(new ActivityValidator());

            }
        }
        public class Handler : IRequestHandler<Command, Result<Unit>>
        {

            private readonly DataContext _context;
            private readonly  IUserAccessor _userAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {

                var user = await _context.Users.FirstOrDefaultAsync(x=>x.UserName==_userAccessor.GetUserName());
                var attendee = new ActivityAttendee{
                    Activity=request.Activity,
                    AppUser = user,
                    IsHost= true
                };

                request.Activity.Attendees.Add(attendee);


                _context.Activities.Add(request.Activity);

                var success = await _context.SaveChangesAsync() > 0;

                if (!success) return Result<Unit>.Failure("Error occured while creating activity");

                return Result<Unit>.Success(Unit.Value);

            }
        }
    }


}