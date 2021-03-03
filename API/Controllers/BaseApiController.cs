using API.Extensions;
using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices
            .GetService<IMediator>();

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null) return NotFound();
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            if (result.IsSuccess && result.Value == null) return NotFound();
            return BadRequest(result.Error);
        }

        protected ActionResult HandlePagedResult<T>(Result<PagedList<T>> result)
        {
            if (result == null) return NotFound();
            if (result.IsSuccess && result.Value != null)
            {
                Response.AddPaginationHeader(result.Value.CurrentPage, result.Value.PageSize, result.Value.TotalCount, result.Value.TotalPages);
                return Ok(result.Value);
            }
            if (result.IsSuccess && result.Value == null) return NotFound();
            return BadRequest(result.Error);
        }
    }


}

//Available in C# 8.0 and later, the null-coalescing assignment operator ??= assigns the value of its right-hand operand 
//to its left-hand operand only if the left-hand operand evaluates to null. 
//The ??= operator doesn't evaluate its right-hand operand if the left-hand operand evaluates to non-null.

// member => expression;

// where expression is a valid expression. The return type of expression must be implicitly convertible to the member's return type. 
// If the member's return type is void or if the member is a constructor, a finalizer, or a property or indexer set accessor, 
// expression must be a statement expression. Because the expression's result is discarded, the return type of that expression can be any type.

// public override string ToString() => $"{fname} {lname}".Trim();

// It's a shorthand version of the following method definition:

// public override string ToString()
// {
//    return $"{fname} {lname}".Trim();
// }