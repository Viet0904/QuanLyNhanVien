-- =============================================
-- Quản Lý Nhân Viên - Tạo Database
-- =============================================
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyNhanVien')
BEGIN
    CREATE DATABASE QuanLyNhanVien
    COLLATE Vietnamese_CI_AS;
END
GO

USE QuanLyNhanVien;
GO
