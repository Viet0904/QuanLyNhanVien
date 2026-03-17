using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class PositionService
    {
        private readonly PositionRepository _posRepo;

        public PositionService(PositionRepository posRepo)
        {
            _posRepo = posRepo;
        }

        public async Task<IEnumerable<Position>> GetAllAsync(bool activeOnly = true)
            => await _posRepo.GetAllAsync(activeOnly);

        public async Task<(bool Success, string Message, int Id)> CreateAsync(Position pos)
        {
            if (string.IsNullOrWhiteSpace(pos.PositionName))
                return (false, "Vui lòng nhập tên chức vụ.", 0);

            var id = await _posRepo.InsertAsync(pos);
            return (true, $"Thêm chức vụ '{pos.PositionName}' thành công!", id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Position pos)
        {
            if (string.IsNullOrWhiteSpace(pos.PositionName))
                return (false, "Vui lòng nhập tên chức vụ.");

            await _posRepo.UpdateAsync(pos);
            return (true, $"Cập nhật chức vụ '{pos.PositionName}' thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int posId)
        {
            if (await _posRepo.HasEmployeesAsync(posId))
                return (false, "Không thể xóa chức vụ vì còn nhân viên đang sử dụng.");

            await _posRepo.DeleteAsync(posId);
            return (true, "Đã vô hiệu hóa chức vụ.");
        }
    }
}
