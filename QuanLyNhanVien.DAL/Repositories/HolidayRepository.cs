using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class HolidayRepository : BaseRepository
    {
        public HolidayRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        /// <summary>
        /// Lấy tất cả ngày lễ active
        /// </summary>
        public async Task<IEnumerable<Holiday>> GetAllAsync(int? year = null)
        {
            var sql = "SELECT * FROM Holidays WHERE IsActive = 1";
            if (year.HasValue) sql += " AND [Year] = @Year";
            sql += " ORDER BY HolidayDate";
            return await QuerySqlAsync<Holiday>(sql, new { Year = year });
        }

        /// <summary>
        /// Lấy tất cả ngày lễ trong 1 khoảng thời gian (để check nghỉ phép)
        /// </summary>
        public async Task<HashSet<DateTime>> GetHolidayDatesAsync(DateTime startDate, DateTime endDate)
        {
            var sql = @"SELECT HolidayDate FROM Holidays 
                        WHERE IsActive = 1 AND HolidayDate BETWEEN @Start AND @End";
            var dates = await QuerySqlAsync<DateTime>(sql, new { Start = startDate, End = endDate });
            return new HashSet<DateTime>(dates.Select(d => d.Date));
        }

        public async Task<int> InsertAsync(Holiday holiday)
        {
            var sql = @"INSERT INTO Holidays (HolidayName, HolidayDate, [Year], IsRecurring, IsActive, Notes)
                        VALUES (@HolidayName, @HolidayDate, @Year, @IsRecurring, @IsActive, @Notes);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, holiday);
        }

        public async Task UpdateAsync(Holiday holiday)
        {
            var sql = @"UPDATE Holidays SET HolidayName = @HolidayName, HolidayDate = @HolidayDate, 
                        [Year] = @Year, IsRecurring = @IsRecurring, IsActive = @IsActive, Notes = @Notes
                        WHERE HolidayId = @HolidayId";
            await ExecuteSqlAsync(sql, holiday);
        }

        public async Task DeleteAsync(int holidayId)
        {
            var sql = "DELETE FROM Holidays WHERE HolidayId = @Id";
            await ExecuteSqlAsync(sql, new { Id = holidayId });
        }
    }
}
