using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class PositionRepository : BaseRepository
    {
        public PositionRepository(DbConnectionFactory factory) : base(factory) { }

        public async Task<IEnumerable<Position>> GetAllAsync(bool activeOnly = true)
        {
            var sql = activeOnly
                ? "SELECT * FROM Positions WHERE IsActive = 1 ORDER BY [Level] DESC, PositionName"
                : "SELECT * FROM Positions ORDER BY [Level] DESC, PositionName";
            return await QuerySqlAsync<Position>(sql);
        }

        public async Task<Position?> GetByIdAsync(int id)
        {
            return (await QuerySqlAsync<Position>(
                "SELECT * FROM Positions WHERE PositionId = @Id", new { Id = id })).FirstOrDefault();
        }

        public async Task<int> InsertAsync(Position pos)
        {
            var sql = @"INSERT INTO Positions (PositionName, [Level], AllowanceAmount, IsActive)
                        VALUES (@PositionName, @Level, @AllowanceAmount, 1);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, pos);
        }

        public async Task UpdateAsync(Position pos)
        {
            var sql = @"UPDATE Positions SET PositionName = @PositionName, [Level] = @Level, 
                        AllowanceAmount = @AllowanceAmount WHERE PositionId = @PositionId";
            await ExecuteSqlAsync(sql, pos);
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteSqlAsync("UPDATE Positions SET IsActive = 0 WHERE PositionId = @Id", new { Id = id });
        }

        public async Task<bool> HasEmployeesAsync(int positionId)
        {
            using var conn = _dbFactory.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Employees WHERE PositionId = @Id AND IsActive = 1",
                new { Id = positionId });
            return count > 0;
        }
    }
}
