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
    public class Delete
    {
        public class Commmand : IRequest<Result<Unit>>{
            
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Commmand, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IPhotoAccessor photoAccessor, IUserAccessor userAccessor)
            {
                _context =context;
                _photoAccessor = photoAccessor; 
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Commmand request, CancellationToken cancellationToken)
            {
                var user =  await _context.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x=>x.Id==_userAccessor.GetUserId());

                if(user==null) return null;

                var photo = user.Photos.FirstOrDefault(x=>x.Id==request.Id);

                if(photo==null) return null;

                if(photo.IsMain) return Result<Unit>.Failure("You can not delete the main photo");

                var result = await _photoAccessor.DeletePhoto(photo.Id);

                if(result == null) return Result<Unit>.Failure("Problem deleting the photo");

                user.Photos.Remove(photo);
                _context.Photos.Remove(photo);

                var success = await _context.SaveChangesAsync()>0;

                if(success) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Problem deleting the photo from API");
            }
        }
    }
}