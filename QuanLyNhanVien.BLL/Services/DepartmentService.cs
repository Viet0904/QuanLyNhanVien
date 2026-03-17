using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class DepartmentService
    {
        private readonly DepartmentRepository _deptRepo;

        public DepartmentService(DepartmentRepository deptRepo)
        {
            _deptRepo = deptRepo;
        }

        public async Task<IEnumerable<Department>> GetAllAsync(bool activeOnly = true)
            => await _deptRepo.GetAllAsync(activeOnly);

        public async Task<Department?> GetByIdAsync(int id) => await _deptRepo.GetByIdAsync(id);

        public async Task<(bool Success, string Message, int Id)> CreateAsync(Department dept)
        {
            if (string.IsNullOrWhiteSpace(dept.DepartmentName))
                return (false, "Vui lòng nhập tên phòng ban.", 0);
            if (string.IsNullOrWhiteSpace(dept.DepartmentCode))
                return (false, "Vui lòng nhập mã phòng ban.", 0);

            var id = await _deptRepo.InsertAsync(dept);
            return (true, $"Thêm phòng ban '{dept.DepartmentName}' thành công!", id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Department dept)
        {
            if (string.IsNullOrWhiteSpace(dept.DepartmentName))
                return (false, "Vui lòng nhập tên phòng ban.");

            await _deptRepo.UpdateAsync(dept);
            return (true, $"Cập nhật phòng ban '{dept.DepartmentName}' thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int deptId)
        {
            var dept = await _deptRepo.GetByIdAsync(deptId);
            if (dept == null) return (false, "Không tìm thấy phòng ban.");

            if (await _deptRepo.HasEmployeesAsync(deptId))
                return (false, $"Không thể xóa phòng ban '{dept.DepartmentName}' vì còn nhân viên.");

            await _deptRepo.DeleteAsync(deptId);
            return (true, $"Đã vô hiệu hóa phòng ban '{dept.DepartmentName}'.");
        }
    }
}
