using System;
using System.Collections.Generic;
using System.Text;

namespace MiniDesk.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
