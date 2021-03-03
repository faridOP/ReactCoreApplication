using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos
{
    public class SetMain
    {
        public class  Command : IRequest<Result<Unit>>
        {
            public string Id {get;set;}
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {

            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor )
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x=>x.Id==_userAccessor.GetUserId());

                if(user==null) return null;

                var previousMainPhoto = user.Photos.FirstOrDefault(x=>x.IsMain);
                if(previousMainPhoto==null) return null;

                previousMainPhoto.IsMain=false;

                var currentPhoto = user.Photos.FirstOrDefault(x=>x.Id==request.Id);

                if(currentPhoto==null) return null;
                currentPhoto.IsMain=true;

                bool result = await _context.SaveChangesAsync()>0;

                if(result) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Problem occured while saving the changes");
            }
        }
    }
}