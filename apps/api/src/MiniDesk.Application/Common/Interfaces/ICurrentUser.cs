using System;
using System.Collections.Generic;
using System.Text;

namespace MiniDesk.Application.Common.Interfaces
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? UserName { get; }
        bool IsInRole(string role);
    }
}
