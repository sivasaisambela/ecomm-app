using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Responses
{
    /// <summary>
    /// A standardized JSON response envelope used across all our microservices
    /// </summary>
    public record ApiResponse<T>(
        bool Success,
        T? Data,
        string Message = ""
    ) where T : class?;
}
