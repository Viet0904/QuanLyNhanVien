using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class AdvanceService
    {
        private readonly AdvanceRepository _repo;

        public AdvanceService(AdvanceRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Advance>> GetAllAsync(int? month = null, int? year = null)
            => await _repo.GetAllAsync(month, year);

        public async Task<decimal> GetTotalAdvanceAsync(int employeeId, int month, int year)
            => await _repo.GetTotalAdvanceAsync(employeeId, month, year);

        public async Task<(bool Ok, string Msg)> CreateAsync(Advance advance)
        {
            if (advance.EmployeeId <= 0) return (false, "Chưa chọn nhân viên.");
            if (advance.Amount <= 0) return (false, "Số tiền phải lớn hơn 0.");

            advance.Status = "Approved";
            await _repo.InsertAsync(advance);
            return (true, "Thêm tạm ứng thành công!");
        }

        public async Task<(bool Ok, string Msg)> UpdateAsync(Advance advance)
        {
            await _repo.UpdateAsync(advance);
            return (true, "Cập nhật thành công!");
        }

        public async Task<(bool Ok, string Msg)> DeleteAsync(int advanceId)
        {
            await _repo.DeleteAsync(advanceId);
            return (true, "Đã xóa tạm ứng.");
        }
    }
}
