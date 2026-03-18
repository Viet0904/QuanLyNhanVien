using System.Net;
using System.Net.Mail;
using QuanLyNhanVien.DAL.Repositories;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Dịch vụ gửi email qua SMTP
    /// </summary>
    public class EmailService
    {
        private readonly CompanySettingsRepository _settingsRepo;

        public EmailService(CompanySettingsRepository settingsRepo)
        {
            _settingsRepo = settingsRepo;
        }

        /// <summary>
        /// Lấy cấu hình SMTP từ CompanySettings
        /// </summary>
        private async Task<SmtpConfig?> GetSmtpConfigAsync()
        {
            var settings = await _settingsRepo.GetAllAsync();
            var dict = settings.ToDictionary(x => x.SettingKey, x => x.SettingValue);

            var host = dict.GetValueOrDefault("SMTP_Host", "");
            if (string.IsNullOrWhiteSpace(host)) return null;

            return new SmtpConfig
            {
                Host = host,
                Port = int.TryParse(dict.GetValueOrDefault("SMTP_Port", "587"), out var p) ? p : 587,
                Username = dict.GetValueOrDefault("SMTP_Username", ""),
                Password = dict.GetValueOrDefault("SMTP_Password", ""),
                EnableSsl = dict.GetValueOrDefault("SMTP_EnableSsl", "true") == "true",
                SenderName = dict.GetValueOrDefault("SMTP_SenderName", ""),
                SenderEmail = dict.GetValueOrDefault("SMTP_SenderEmail", "")
            };
        }

        /// <summary>
        /// Gửi email (text hoặc HTML)
        /// </summary>
        public async Task<(bool Success, string Message)> SendAsync(
            string toEmail, string subject, string body, bool isHtml = false)
        {
            var config = await GetSmtpConfigAsync();
            if (config == null)
                return (false, "Chưa cấu hình SMTP. Vui lòng vào Thiết Lập Công Ty → cấu hình SMTP trước.");

            if (string.IsNullOrWhiteSpace(toEmail))
                return (false, "Địa chỉ email người nhận không được để trống.");

            try
            {
                using var smtp = new SmtpClient(config.Host, config.Port)
                {
                    Credentials = new NetworkCredential(config.Username, config.Password),
                    EnableSsl = config.EnableSsl,
                    Timeout = 15000 // 15s
                };

                var senderEmail = !string.IsNullOrWhiteSpace(config.SenderEmail) ? config.SenderEmail : config.Username;
                var senderName = !string.IsNullOrWhiteSpace(config.SenderName) ? config.SenderName : "Quản Lý Nhân Viên";

                var mailMsg = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMsg.To.Add(toEmail);

                await smtp.SendMailAsync(mailMsg);
                return (true, $"Đã gửi email thành công đến {toEmail}");
            }
            catch (SmtpException ex)
            {
                return (false, $"Lỗi SMTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi gửi email: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi phiếu lương qua email
        /// </summary>
        public async Task<(bool Success, string Message)> SendPayslipAsync(
            string toEmail, string employeeName, string payslipHtml)
        {
            var subject = $"Phiếu lương - {employeeName} - {DateTime.Now:MM/yyyy}";
            return await SendAsync(toEmail, subject, payslipHtml, isHtml: true);
        }

        /// <summary>
        /// Test kết nối SMTP
        /// </summary>
        public async Task<(bool Success, string Message)> TestConnectionAsync(string testEmail)
        {
            return await SendAsync(testEmail, "Test kết nối SMTP",
                "<h3>🎉 Kết nối SMTP thành công!</h3><p>Email này được gửi từ hệ thống Quản Lý Nhân Viên để kiểm tra cấu hình SMTP.</p>",
                isHtml: true);
        }

        /// <summary>
        /// Tạo HTML phiếu lương
        /// </summary>
        public static string BuildPayslipHtml(Models.Entities.SalaryRecord sr, string companyName = "")
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family:Segoe UI,sans-serif;max-width:700px;margin:auto;'>
<div style='background:#1a1a2e;color:#fff;padding:20px;text-align:center;'>
  <h2>{(string.IsNullOrWhiteSpace(companyName) ? "PHIẾU LƯƠNG" : companyName)}</h2>
  <p>Tháng {sr.Month}/{sr.Year}</p>
</div>
<div style='padding:20px;'>
  <table style='width:100%;border-collapse:collapse;'>
    <tr><td style='padding:8px;border-bottom:1px solid #eee;'><b>Nhân viên:</b></td><td style='padding:8px;border-bottom:1px solid #eee;'>{sr.EmployeeName} ({sr.EmployeeCode})</td></tr>
    <tr><td style='padding:8px;border-bottom:1px solid #eee;'><b>Phòng ban:</b></td><td style='padding:8px;border-bottom:1px solid #eee;'>{sr.DepartmentName}</td></tr>
    <tr><td style='padding:8px;border-bottom:1px solid #eee;'><b>Ngày công:</b></td><td style='padding:8px;border-bottom:1px solid #eee;'>{sr.WorkingDays}/{sr.StandardDays}</td></tr>
  </table>
  <h3 style='margin-top:20px;'>💰 Chi tiết lương</h3>
  <table style='width:100%;border-collapse:collapse;'>
    <tr><td style='padding:6px;'>Lương cơ bản × Hệ số:</td><td style='text-align:right;'>{sr.BasicSalary:N0} × {sr.SalaryCoefficient}</td></tr>
    <tr><td style='padding:6px;'>Phụ cấp chức vụ:</td><td style='text-align:right;'>{sr.PositionAllowance:N0}</td></tr>
    <tr><td style='padding:6px;'>Phụ cấp khác:</td><td style='text-align:right;'>{sr.OtherAllowance:N0}</td></tr>
    <tr><td style='padding:6px;'>Lương OT:</td><td style='text-align:right;'>{sr.OvertimePay:N0}</td></tr>
    <tr style='font-weight:bold;background:#f0f0f0;'><td style='padding:6px;'>Tổng thu nhập:</td><td style='text-align:right;'>{sr.GrossIncome:N0}</td></tr>
    <tr><td style='padding:6px;'>BHXH (8%):</td><td style='text-align:right;color:red;'>-{sr.SocialInsurance:N0}</td></tr>
    <tr><td style='padding:6px;'>BHYT (1.5%):</td><td style='text-align:right;color:red;'>-{sr.HealthInsurance:N0}</td></tr>
    <tr><td style='padding:6px;'>BHTN (1%):</td><td style='text-align:right;color:red;'>-{sr.UnemploymentInsurance:N0}</td></tr>
    <tr><td style='padding:6px;'>Thuế TNCN:</td><td style='text-align:right;color:red;'>-{sr.PersonalIncomeTax:N0}</td></tr>
    <tr><td style='padding:6px;'>Khấu trừ khác:</td><td style='text-align:right;color:red;'>-{sr.OtherDeductions:N0}</td></tr>
    <tr style='font-weight:bold;background:#e8f5e9;font-size:16px;'><td style='padding:10px;'>THỰC LĨNH:</td><td style='text-align:right;color:green;'>{sr.NetSalary:N0} VNĐ</td></tr>
  </table>
</div>
<div style='background:#f5f5f5;padding:10px;text-align:center;font-size:12px;color:#888;'>
  Phiếu lương được gửi tự động từ hệ thống Quản Lý Nhân Viên.
</div>
</body>
</html>";
        }

        private class SmtpConfig
        {
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; } = 587;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool EnableSsl { get; set; } = true;
            public string SenderName { get; set; } = string.Empty;
            public string SenderEmail { get; set; } = string.Empty;
        }
    }
}
