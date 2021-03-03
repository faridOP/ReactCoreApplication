using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>{
            public string DisplayName { get; set; }
            public string Bio { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext context;
            private readonly IUserAccessor userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                this.context = context;
                this.userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await context.Users.FirstOrDefaultAsync(x=>x.UserName==userAccessor.GetUserName());

                if(user ==null) return null;

                user.Bio=request.Bio;
                user.DisplayName = request.DisplayName;

               context.Entry(user).State = EntityState.Modified;

                var result = await context.SaveChangesAsync()>0;

                if(result) return Result<Unit>.Success(Unit.Value);
                
                return Result<Unit>.Failure("Problem occured while updating the user profile");
            }
        }
    }
}