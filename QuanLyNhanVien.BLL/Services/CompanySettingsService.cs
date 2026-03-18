using QuanLyNhanVien.DAL.Repositories;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Quản lý thiết lập công ty
    /// </summary>
    public class CompanySettingsService
    {
        private readonly CompanySettingsRepository _repo;

        // Các key chuẩn
        public const string KEY_COMPANY_NAME = "CompanyName";
        public const string KEY_ADDRESS = "Address";
        public const string KEY_PHONE = "Phone";
        public const string KEY_EMAIL = "Email";
        public const string KEY_TAX_CODE = "TaxCode";
        public const string KEY_WEBSITE = "Website";
        public const string KEY_FAX = "Fax";
        public const string KEY_REPRESENTATIVE = "Representative";
        public const string KEY_REPRESENTATIVE_TITLE = "RepresentativeTitle";

        public CompanySettingsService(CompanySettingsRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> GetAsync(string key, string defaultValue = "")
        {
            var value = await _repo.GetByKeyAsync(key);
            return value ?? defaultValue;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.ToDictionary(x => x.SettingKey, x => x.SettingValue);
        }

        public async Task<(bool Ok, string Msg)> SaveAllAsync(Dictionary<string, string> settings)
        {
            try
            {
                await _repo.SaveAllAsync(settings);
                return (true, "Lưu thiết lập thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi lưu thiết lập: {ex.Message}");
            }
        }
    }
}
