using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class ContractService
    {
        private readonly ContractRepository _repo;

        public ContractService(ContractRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(int? employeeId = null, bool? isActive = null)
            => await _repo.GetAllAsync(employeeId, isActive);

        public async Task<IEnumerable<Contract>> GetExpiringAsync(int daysAhead = 30)
            => await _repo.GetExpiringAsync(daysAhead);

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
