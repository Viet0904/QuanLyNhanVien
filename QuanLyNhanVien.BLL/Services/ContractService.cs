using QuanLyNhanVien.BLL.Interfaces;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class ContractService : IContractService
    {
        private readonly ContractRepository _repo;
        private readonly EmployeeRepository _empRepo;

        public ContractService(ContractRepository repo, EmployeeRepository empRepo)
        {
            _repo = repo;
            _empRepo = empRepo;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(int? employeeId = null, bool? isActive = null)
            => await _repo.GetAllAsync(employeeId, isActive);

        public async Task<IEnumerable<Contract>> GetExpiringAsync(int daysAhead = 30)
            => await _repo.GetExpiringAsync(daysAhead);

        /// <summary>
        /// MISS-4: Lấy danh sách HĐ đã hết hạn mà NV vẫn active (không có HĐ mới)
        /// </summary>
        public async Task<IEnumerable<Contract>> GetExpiredActiveContractsAsync()
            => await _repo.GetExpiredActiveAsync();

        /// <summary>
        /// MISS-4: Tự động deactivate NV có HĐ hết hạn mà không có HĐ mới
        /// </summary>
        public async Task<(bool Ok, string Msg, int Count)> DeactivateExpiredEmployeesAsync()
        {
            var expired = (await _repo.GetExpiredActiveAsync()).ToList();
            if (!expired.Any())
                return (true, "Không có nhân viên nào cần cập nhật.", 0);

            int count = 0;
            foreach (var contract in expired)
            {
                await _empRepo.DeactivateAsync(contract.EmployeeId);
                contract.IsActive = false;
                await _repo.UpdateAsync(contract);
                count++;
            }
            return (true, $"Đã deactivate {count} nhân viên hết hạn hợp đồng.", count);
        }

        public async Task<(bool Ok, string Msg)> CreateAsync(Contract contract)
        {
            if (contract.EmployeeId <= 0) return (false, "Chưa chọn nhân viên.");
            if (string.IsNullOrWhiteSpace(contract.ContractType)) return (false, "Chưa chọn loại hợp đồng.");

            // Auto generate contract code
            contract.ContractCode = $"HD-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
            await _repo.InsertAsync(contract);
            return (true, "Thêm hợp đồng thành công!");
        }

        public async Task<(bool Ok, string Msg)> UpdateAsync(Contract contract)
        {
            await _repo.UpdateAsync(contract);
            return (true, "Cập nhật hợp đồng thành công!");
        }

        public async Task<(bool Ok, string Msg)> DeleteAsync(int contractId)
        {
            await _repo.DeleteAsync(contractId);
            return (true, "Đã hủy hợp đồng.");
        }
    }
}
