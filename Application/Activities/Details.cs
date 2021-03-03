using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Activities
{
    public class Details
    {
        public class Query : IRequest<Result<ActivityDto>>
        {
            public Guid Id { get; set; }
        };

        public class Handler : IRequestHandler<Query, Result<ActivityDto>>
        {
            private readonly DataContext _context;


            private readonly IMapper _mapper;
            private readonly IUserAccessor userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                this.userAccessor = userAccessor;
                _context = context;
                _mapper = mapper;
            }
            public async Task<Result<ActivityDto>> Handle(Query request, CancellationToken cancellationToken)
            {


                var activity = await _context.Activities
                .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider, new{currentUsername=userAccessor.GetUserName()})
                .FirstOrDefaultAsync(a => a.Id == request.Id);

                if (activity == null)
                {
                    return null;
                }

                return Result<ActivityDto>.Success(activity);
            }
        }
    }
}