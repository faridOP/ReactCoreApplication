using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;
using Application.Activities;

namespace Application.Activities
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>> // We shall send the request with these datas
        {
           public Activity Activity { get; set; }
        }

        
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x=>x.Activity).SetValidator(new ActivityValidator());
                
            }
        }

        public class Handler : IRequestHandler<Command,Result<Unit>>
        {

            private readonly DataContext _context;
            private IMapper _mapper;
            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper=  mapper;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //Request handling logic goes here
                var activity = await _context.Activities.FindAsync(request.Activity.Id);

                if(activity ==null)  return  null;

                _mapper.Map(request.Activity,activity);

                var success = await _context.SaveChangesAsync() > 0;
                
                if(!success) return  Result<Unit>.Failure("Could not save changes. This might happen due to the following reasons:"+
                " \n - There was an error in the application's database"+
                " \n - No changes occured to save to the database");

                return Result<Unit>.Success(Unit.Value);
               
            }
        }
    }
}