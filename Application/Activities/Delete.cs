using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Domain;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }

        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {

            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //Request handling logic goes here

                Activity activity = await _context.Activities.FindAsync(request.Id);

                if(activity==null) return null;

                _context.Activities.Remove(activity);

                bool success = await _context.SaveChangesAsync() > 0;

                if(!success) return Result<Unit>.Failure("Could not delete activity");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}