using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class Create
    {
        public class Command : IRequest<Result<CommentDto>>
        {
            public string Body {get;set;}
            public Guid ActivityId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>{
            public CommandValidator(){
                RuleFor(x=>x.Body).NotEmpty();

            }
        }

        public class Handler : IRequestHandler<Command, Result<CommentDto>>
        {
            private readonly DataContext context;
            private readonly IMapper mapper;
            private readonly IUserAccessor userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor){
                this.context = context;
                this.mapper = mapper;
                this.userAccessor = userAccessor;
            }
            public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await context.Activities.FirstOrDefaultAsync(x=>x.Id==request.ActivityId);

                if(activity == null) return null;

                var user = await context.Users
                .Include(p=>p.Photos)
                .SingleOrDefaultAsync(x=>x.UserName==userAccessor.GetUserName());

                if(user == null) return null;

                Comment comment = new Comment{
                    Activity=activity,
                    Author=user,
                    Body=request.Body,
                };

                activity.Comments.Add(comment);

                var result = await context.SaveChangesAsync()>0;

                // CommentDto commentDto = new CommentDto{
                //     Body=comment.Body,
                //     CreatedAt=comment.CreatedAt,
                //     DisplayName=comment.Author.DisplayName,
                //     Id=comment.Id,
                //     Image=comment.Author.Photos.FirstOrDefault(x=>x.IsMain).Url,
                //     Username=comment.Author.UserName
                // };

                if(result) return Result<CommentDto>.Success(mapper.Map<CommentDto>(comment));

                return Result<CommentDto>.Failure("Problem occured while adding comment");

            }
        }
    }
}