using AdminService.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminService.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
        Task LogDashboardViewedAsync(string performedBy, CancellationToken cancellationToken = default);
    }
}
