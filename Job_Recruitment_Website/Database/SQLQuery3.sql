USE RecruitmentDB;
GO

-- Thêm 3 Nhà tuyển dụng (Employer - RoleId = 2) gắn với FPT Software (1), VNG (2), Viettel Solutions (3)
IF NOT EXISTS (SELECT 1 FROM [dbo].[USERS] WHERE [Email] = 'dang.quoc.huy@fpt.com.vn')
BEGIN
    INSERT INTO [dbo].[USERS] ([Email], [PasswordHash], [FullName], [Phone], [RoleId], [IsActive], [CreatedAt], [CompanyId]) VALUES
    ('dang.quoc.huy@fpt.com.vn', '$2b$12$HashPass006', N'Đặng Quốc Huy', '0944556677', 2, 1, '2025-01-22', 1),
    ('vu.thanh.mai@vng.com.vn', '$2b$12$HashPass007', N'Vũ Thanh Mai', '0955667788', 2, 1, '2025-01-24', 2),
    ('bui.duc.anh@viettel.com.vn', '$2b$12$HashPass008', N'Bùi Đức Anh', '0966778899', 2, 1, '2025-01-26', 3);
END
GO