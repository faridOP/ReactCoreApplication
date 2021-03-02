using System.Collections.Generic;
using Application.Core;
using MediatR;
using System;
using System.Threading.Tasks;
using System.Threading;
using Persistence;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper;

namespace Application.Comments
{
    public class List
    {
        public class Query:IRequest<Result<List<CommentDto>>>
        {
            public Guid ActivityId {get;set;}
        }


        public class Handler : IRequestHandler<Query, Result<List<CommentDto>>>
        {
            private readonly DataContext context;
            private readonly IMapper mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                this.context = context;
                this.mapper = mapper;
            }

            public async Task<Result<List<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
              var comments = await context.Comments
              .Where(x=>x.Activity.Id == request.ActivityId)
              .OrderBy(x=>x.CreatedAt)
              .ProjectTo<CommentDto>(mapper.ConfigurationProvider)
              .ToListAsync();

              return Result<List<CommentDto>>.Success(comments);

            }
        }
    }
}