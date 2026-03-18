using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class EmployeeEventService
    {
        private readonly EmployeeEventRepository _repo;

        public EmployeeEventService(EmployeeEventRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<EmployeeEvent>> GetAllAsync(int? employeeId = null, string? eventType = null)
            => await _repo.GetAllAsync(employeeId, eventType);

        public async Task<(bool Ok, string Msg)> CreateAsync(EmployeeEvent ev)
        {
            if (ev.EmployeeId <= 0) return (false, "Chưa chọn nhân viên.");
            if (string.IsNullOrWhiteSpace(ev.EventType)) return (false, "Chưa chọn loại sự kiện.");
            if (string.IsNullOrWhiteSpace(ev.Description)) return (false, "Vui lòng nhập mô tả.");

            await _repo.InsertAsync(ev);
            return (true, "Thêm sự kiện thành công!");
        }

        public async Task<(bool Ok, string Msg)> UpdateAsync(EmployeeEvent ev)
        {
            await _repo.UpdateAsync(ev);
            return (true, "Cập nhật thành công!");
        }

        public async Task<(bool Ok, string Msg)> DeleteAsync(int eventId)
        {
            await _repo.DeleteAsync(eventId);
            return (true, "Đã xóa sự kiện.");
        }
    }
}
