using System;
namespace Application.Interfaces
{
    public interface IUserAccessor
    {
         string GetUserName();
         string GetUserId();
    }
}