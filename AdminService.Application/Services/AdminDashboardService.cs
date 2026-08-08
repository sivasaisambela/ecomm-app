using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;


namespace AdminService.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _repository;

        public AdminDashboardService(IAdminDashboardRepository repository)
        {
            _repository = repository;
        }

        public Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AdminDashboardResponse
            {
                TotalProducts = 0,
                TotalOrders = 0,
                TotalCustomers = 0,
                TotalRevenue = 0,
                LowStockProducts = 0
            });
        }

        public async Task LogDashboardViewedAsync(string performedBy, CancellationToken cancellationToken = default)
        {
            var log = new AuditLog(
                Guid.NewGuid(),
                "DashboardViewed",
                performedBy,
                "AdminDashboard",
                null,
                "Admin dashboard was viewed");

            await _repository.AddAuditLogAsync(log, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}
