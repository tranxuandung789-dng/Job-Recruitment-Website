USE RecruitmentDB;
GO

INSERT INTO [dbo].[JOBS] ([CompanyId], [Title], [Description], [SalaryRange], [Location], [Deadline], [Status], [PostedDate], [CategoryId]) VALUES
(2, N'Chuyên viên Thiết kế UI/UX (Figma)', N'Thiết kế giao diện người dùng và trải nghiệm ứng dụng di động, phối hợp chặt chẽ cùng đội ngũ Product và Frontend.', N'18 - 28 triệu VND', N'TP. Hồ Chí Minh', '2026-12-25', N'Open', '2025-02-14', 5),
(1, N'Kỹ sư Kiểm thử Phần mềm (QA/Tester)', N'Xây dựng kịch bản kiểm thử, thực hiện kiểm thử chức năng và tự động hóa cho các dự án phần mềm doanh nghiệp.', N'14 - 22 triệu VND', N'Hà Nội', '2026-12-28', N'Open', '2025-02-15', 1);
GO