using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _empRepo;
        private readonly DepartmentRepository _deptRepo;
        private readonly PositionRepository _posRepo;

        public EmployeeService(EmployeeRepository empRepo, DepartmentRepository deptRepo, PositionRepository posRepo)
        {
            _empRepo = empRepo;
            _deptRepo = deptRepo;
            _posRepo = posRepo;
        }

        public async Task<PaginationResult<Employee>> GetListAsync(int page = 1, int pageSize = 50,
            string? search = null, int? deptId = null, bool? isActive = null)
        {
            return await _empRepo.GetAllAsync(page, pageSize, search, deptId, isActive);
        }

        public async Task<Employee?> GetByIdAsync(int id) => await _empRepo.GetByIdAsync(id);

        public async Task<(bool Success, string Message, int EmployeeId)> CreateAsync(Employee emp)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(emp.FullName))
                return (false, "Vui lòng nhập họ tên nhân viên.", 0);
            if (emp.DepartmentId <= 0)
                return (false, "Vui lòng chọn phòng ban.", 0);
            if (emp.PositionId <= 0)
                return (false, "Vui lòng chọn chức vụ.", 0);

            // Auto-generate code
            emp.EmployeeCode = await _empRepo.GenerateNextCodeAsync();

            var id = await _empRepo.InsertAsync(emp);
            return (true, $"Thêm nhân viên {emp.FullName} thành công! Mã: {emp.EmployeeCode}", id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Employee emp)
        {
            if (string.IsNullOrWhiteSpace(emp.FullName))
                return (false, "Vui lòng nhập họ tên nhân viên.");

            emp.UpdatedAt = DateTime.Now;
            await _empRepo.UpdateAsync(emp);
            return (true, $"Cập nhật nhân viên {emp.FullName} thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int employeeId)
        {
            var emp = await _empRepo.GetByIdAsync(employeeId);
            if (emp == null) return (false, "Không tìm thấy nhân viên.");

            await _empRepo.DeleteAsync(employeeId);
            return (true, $"Đã vô hiệu hóa nhân viên {emp.FullName}.");
        }

        public async Task<IEnumerable<Department>> GetDepartmentsAsync() => await _deptRepo.GetAllAsync();
        public async Task<IEnumerable<Position>> GetPositionsAsync() => await _posRepo.GetAllAsync();
    }
}
