using Dapper;
using QuanLyNhanVien.DAL.Context;
using System.Data;

namespace QuanLyNhanVien.DAL.Repositories
{
    /// <summary>
    /// Base repository với các phương thức CRUD chung dùng Dapper
    /// </summary>
    public abstract class BaseRepository
    {
        protected readonly DbConnectionFactory _dbFactory;

        protected BaseRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Thực thi Stored Procedure trả về danh sách
        /// </summary>
        protected async Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters = null)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Thực thi Stored Procedure trả về 1 record
        /// </summary>
        protected async Task<T?> QuerySingleOrDefaultAsync<T>(string spName, object? parameters = null)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Thực thi Stored Procedure trả về scalar
        /// </summary>
        protected async Task<T?> ExecuteScalarAsync<T>(string spName, object? parameters = null)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Thực thi Stored Procedure không trả về giá trị
        /// </summary>
        protected async Task<int> ExecuteAsync(string spName, object? parameters = null)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Thực thi SQL query trực tiếp (cho các trường hợp đặc biệt)
        /// </summary>
        protected async Task<IEnumerable<T>> QuerySqlAsync<T>(string sql, object? parameters = null)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<T>(sql, parameters);
        }

        /// <summary>
        /// Thực thi SQL trực tiếp không trả về giá trị
        /// </summary>
        protected async Task<int> ExecuteSqlAsync(string sql, object? parameters = null)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, parameters);
        }

        /// <summary>
        /// Thực thi SP trả về nhiều result sets (dùng QueryMultiple).
        /// Connection được tự động dispose sau khi callback hoàn tất.
        /// </summary>
        protected async Task<T> QueryMultipleAsync<T>(string spName, object? parameters, Func<SqlMapper.GridReader, Task<T>> callback)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var multi = await conn.QueryMultipleAsync(spName, parameters, commandType: CommandType.StoredProcedure);
            return await callback(multi);
        }
    }
}
