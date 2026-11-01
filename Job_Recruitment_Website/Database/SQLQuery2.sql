-- =================================================================================
-- SQL SERVER (T-SQL)
-- =================================================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RecruitmentDB')
BEGIN
    ALTER DATABASE RecruitmentDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RecruitmentDB;
END
GO

CREATE DATABASE RecruitmentDB;
GO

USE RecruitmentDB;
GO

-- =================================================================================
-- PHẦN 1: KHỞI TẠO 7 BẢNG
-- =================================================================================

DROP TABLE IF EXISTS [dbo].[APPLICATIONS];
DROP TABLE IF EXISTS [dbo].[JOBS];
DROP TABLE IF EXISTS [dbo].[CATEGORIES];
DROP TABLE IF EXISTS [dbo].[CANDIDATEPROFILES];
IF OBJECT_ID('dbo.FK_COMPANIES_USERS', 'F') IS NOT NULL
    ALTER TABLE [dbo].[COMPANIES] DROP CONSTRAINT [FK_COMPANIES_USERS];
DROP TABLE IF EXISTS [dbo].[USERS];
DROP TABLE IF EXISTS [dbo].[COMPANIES];
DROP TABLE IF EXISTS [dbo].[ROLES];
GO

-- 1. Bảng ROLES (Vai trò người dùng)
CREATE TABLE [dbo].[ROLES]
(
    [RoleId]   INT IDENTITY(1,1) NOT NULL,
    [RoleName] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_ROLES] PRIMARY KEY CLUSTERED ([RoleId] ASC),
    CONSTRAINT [UQ_ROLES_RoleName] UNIQUE ([RoleName]),
    CONSTRAINT [CK_ROLES_RoleId] CHECK ([RoleId] > 0),
    CONSTRAINT [CK_ROLES_RoleName] CHECK (LEN(LTRIM(RTRIM([RoleName]))) > 0)
);
GO

-- 2. Bảng COMPANIES (Thông tin công ty tuyển dụng)
CREATE TABLE [dbo].[COMPANIES]
(
    [CompanyId]   INT IDENTITY(1,1) NOT NULL,
    [UserId]      INT           NOT NULL,
    [CompanyName] NVARCHAR(200) NOT NULL,
    [Logo_Url]    VARCHAR(255)  NULL,
    [Address]     NVARCHAR(255) NOT NULL,
    [Website]     VARCHAR(255)  NULL,
    [Description] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_COMPANIES] PRIMARY KEY CLUSTERED ([CompanyId] ASC),
    CONSTRAINT [UQ_COMPANIES_CompanyName] UNIQUE ([CompanyName]),
    CONSTRAINT [CK_COMPANIES_CompanyId] CHECK ([CompanyId] > 0)
);
GO

-- 3. Bảng USERS (Tài khoản người dùng)
CREATE TABLE [dbo].[USERS]
(
    [UserId]       INT IDENTITY(1,1) NOT NULL,
    [Email]        VARCHAR(150)  NOT NULL,
    [PasswordHash] VARCHAR(255)  NOT NULL,
    [FullName]     NVARCHAR(150) NOT NULL,
    [Phone]        VARCHAR(20)   NOT NULL,
    [RoleId]       INT           NOT NULL,
    [IsActive]     BIT           NOT NULL CONSTRAINT [DF_USERS_IsActive] DEFAULT (1),
    [CreatedAt]    DATE          NOT NULL CONSTRAINT [DF_USERS_CreatedAt] DEFAULT (CAST(GETDATE() AS DATE)),
    [CompanyId]    INT           NULL,
    CONSTRAINT [PK_USERS] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [UQ_USERS_Email] UNIQUE ([Email]),
    CONSTRAINT [UQ_USERS_Phone] UNIQUE ([Phone]),
    CONSTRAINT [CK_USERS_UserId] CHECK ([UserId] > 0),
    CONSTRAINT [CK_USERS_Email] CHECK ([Email] LIKE '%_@__%.__%'),
    CONSTRAINT [CK_USERS_Phone] CHECK (LEN([Phone]) BETWEEN 10 AND 11 AND [Phone] NOT LIKE '%[^0-9]%'),
    CONSTRAINT [FK_USERS_ROLES] FOREIGN KEY ([RoleId])
        REFERENCES [dbo].[ROLES]([RoleId]) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT [FK_USERS_COMPANIES] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[COMPANIES]([CompanyId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

-- 4. Bảng CANDIDATEPROFILES (Hồ sơ chi tiết của ứng viên)
CREATE TABLE [dbo].[CANDIDATEPROFILES]
(
    [ProfileId]  INT IDENTITY(1,1) NOT NULL,
    [UserId]     INT           NOT NULL,
    [Education]  NVARCHAR(255) NOT NULL,
    [Skills]     NVARCHAR(255) NOT NULL,
    [CV_Url]     VARCHAR(255)  NOT NULL,
    [UpdatedAt]  DATE          NOT NULL CONSTRAINT [DF_CANDIDATEPROFILES_UpdatedAt] DEFAULT (CAST(GETDATE() AS DATE)),
    [Experience] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_CANDIDATEPROFILES] PRIMARY KEY CLUSTERED ([ProfileId] ASC),
    CONSTRAINT [UQ_CANDIDATEPROFILES_UserId] UNIQUE ([UserId]),
    CONSTRAINT [CK_CANDIDATEPROFILES_ProfileId] CHECK ([ProfileId] > 0),
    CONSTRAINT [FK_CANDIDATEPROFILES_USERS] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[USERS]([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

-- 5. Bảng CATEGORIES (Danh mục nhóm ngành nghề tuyển dụng)
CREATE TABLE [dbo].[CATEGORIES]
(
    [CategoryId]   INT IDENTITY(1,1) NOT NULL,
    [CategoryName] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_CATEGORIES] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
    CONSTRAINT [UQ_CATEGORIES_CategoryName] UNIQUE ([CategoryName]),
    CONSTRAINT [CK_CATEGORIES_CategoryId] CHECK ([CategoryId] > 0),
    CONSTRAINT [CK_CATEGORIES_CategoryName] CHECK (LEN(LTRIM(RTRIM([CategoryName]))) > 0)
);
GO

-- 6. Bảng JOBS (Tin đăng tuyển dụng việc làm)
CREATE TABLE [dbo].[JOBS]
(
    [JobId]       INT IDENTITY(1,1) NOT NULL,
    [CompanyId]   INT           NOT NULL,
    [Title]       NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [SalaryRange] NVARCHAR(100) NOT NULL,
    [Location]    NVARCHAR(150) NOT NULL,
    [Deadline]    DATE          NOT NULL,
    [Status]      NVARCHAR(50)  NOT NULL CONSTRAINT [DF_JOBS_Status] DEFAULT (N'Open'),
    [PostedDate]  DATE          NOT NULL CONSTRAINT [DF_JOBS_PostedDate] DEFAULT (CAST(GETDATE() AS DATE)),
    [CategoryId]  INT           NOT NULL,
    CONSTRAINT [PK_JOBS] PRIMARY KEY CLUSTERED ([JobId] ASC),
    CONSTRAINT [CK_JOBS_JobId] CHECK ([JobId] > 0),
    CONSTRAINT [CK_JOBS_Dates] CHECK ([Deadline] >= [PostedDate]),
    CONSTRAINT [CK_JOBS_Status] CHECK ([Status] IN (N'Open', N'Closed', N'Draft', N'Paused')),
    CONSTRAINT [FK_JOBS_COMPANIES] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[COMPANIES]([CompanyId]) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT [FK_JOBS_CATEGORIES] FOREIGN KEY ([CategoryId])
        REFERENCES [dbo].[CATEGORIES]([CategoryId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

-- 7. Bảng APPLICATIONS (Đơn ứng tuyển của ứng viên)
CREATE TABLE [dbo].[APPLICATIONS]
(
    [ApplicationId] INT IDENTITY(1,1) NOT NULL,
    [JobId]         INT           NOT NULL,
    [CandidateId]   INT           NULL,
    [CoverLetter]   NVARCHAR(MAX) NULL,
    [AppliedDate]   DATE          NOT NULL CONSTRAINT [DF_APPLICATIONS_AppliedDate] DEFAULT (CAST(GETDATE() AS DATE)),
    [Status]        NVARCHAR(50)  NOT NULL CONSTRAINT [DF_APPLICATIONS_Status] DEFAULT (N'Pending'),
    [UserId]        INT           NOT NULL,
    [CV_Url]        VARCHAR(255)  NULL,
    CONSTRAINT [PK_APPLICATIONS] PRIMARY KEY CLUSTERED ([ApplicationId] ASC),
    CONSTRAINT [UQ_APPLICATIONS_Job_User] UNIQUE ([JobId], [UserId]),
    CONSTRAINT [CK_APPLICATIONS_ApplicationId] CHECK ([ApplicationId] > 0),
    CONSTRAINT [CK_APPLICATIONS_Status] CHECK ([Status] IN (N'Pending', N'Submitted', N'Reviewed', N'Interview', N'Accepted', N'Approved', N'Rejected')),
    CONSTRAINT [FK_APPLICATIONS_JOBS] FOREIGN KEY ([JobId])
        REFERENCES [dbo].[JOBS]([JobId]) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT [FK_APPLICATIONS_USERS] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[USERS]([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

-- =================================================================================
-- PHẦN 2: THÊM DỮ LIỆU MẪU
-- =================================================================================

-- 1. Thêm dữ liệu bảng ROLES (3 vai trò chuẩn)
INSERT INTO [dbo].[ROLES] ([RoleName]) VALUES
(N'Admin'),
(N'Recruiter'),
(N'Candidate');
GO

-- 2. Thêm dữ liệu bảng COMPANIES (5 công ty)
INSERT INTO [dbo].[COMPANIES] ([UserId], [CompanyName], [Logo_Url], [Address], [Website], [Description]) VALUES
(1, N'FPT Software', 'https://fptsoftware.com/logo.png', N'Khu Công nghệ cao Quận 9, TP.HCM', 'https://fptsoftware.com', N'Công ty xuất khẩu phần mềm và chuyển đổi số hàng đầu Việt Nam.'),
(2, N'VNG Corporation', 'https://vng.com.vn/logo.png', N'Z06 Đường số 13, Quận 7, TP.HCM', 'https://vng.com.vn', N'Tập đoàn công nghệ kỳ lân chuyên về Zalo, AI, Cloud và trò chơi trực tuyến.'),
(3, N'Viettel Solutions', 'https://solutions.viettel.vn/logo.png', N'Số 1 Trần Hữu Dực, Nam Từ Liêm, Hà Nội', 'https://solutions.viettel.vn', N'Tổng công ty Giải pháp Doanh nghiệp Viettel, tiên phong chuyển đổi số.'),
(4, N'MoMo (M_Service)', 'https://momo.vn/logo.png', N'Phú Mỹ Hưng Tower, Quận 7, TP.HCM', 'https://momo.vn', N'Siêu ứng dụng tài chính và thanh toán điện tử hàng đầu Việt Nam.'),
(5, N'Tiki Corporation', 'https://tiki.vn/logo.png', N'52 Út Tịch, Quận Tân Bình, TP.HCM', 'https://tiki.vn', N'Nền tảng thương mại điện tử và chuỗi cung ứng kho vận thông minh.');
GO

-- 3. Thêm dữ liệu bảng USERS (5 người dùng)
INSERT INTO [dbo].[USERS] ([Email], [PasswordHash], [FullName], [Phone], [RoleId], [IsActive], [CreatedAt], [CompanyId]) VALUES
('nguyen.van.an@gmail.com', '$2b$12$HashPass001', N'Nguyễn Văn An', '0901234567', 1, 1, '2025-01-10', 1),
('tran.thi.bich@gmail.com', '$2b$12$HashPass002', N'Trần Thị Bích', '0912345678', 3, 1, '2025-01-12', 2),
('le.hoang.nam@gmail.com', '$2b$12$HashPass003', N'Lê Hoàng Nam', '0987654321', 3, 1, '2025-01-15', 3),
('pham.minh.tuan@gmail.com', '$2b$12$HashPass004', N'Phạm Minh Tuấn', '0933445566', 2, 1, '2025-01-18', 4),
('hoang.thu.trang@gmail.com', '$2b$12$HashPass005', N'Hoàng Thu Trang', '0977889900', 3, 1, '2025-01-20', 5);
GO

-- Thiết lập khóa ngoại từ COMPANIES(UserId) tham chiếu tới USERS(UserId)
ALTER TABLE [dbo].[COMPANIES]
ADD CONSTRAINT [FK_COMPANIES_USERS] FOREIGN KEY ([UserId])
    REFERENCES [dbo].[USERS]([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- 4. Thêm dữ liệu bảng CANDIDATEPROFILES (5 hồ sơ)
INSERT INTO [dbo].[CANDIDATEPROFILES] ([UserId], [Education], [Skills], [CV_Url], [UpdatedAt], [Experience]) VALUES
(1, N'Cử nhân CNTT - ĐH Bách Khoa', N'C#, ASP.NET MVC, SQL Server', 'https://cv.vn/nguyen-van-an.pdf', '2025-02-01', N'3 năm kinh nghiệm Backend Developer'),
(2, N'Cử nhân KHMT - ĐH KHTN TP.HCM', N'ReactJS, TypeScript, Next.js, TailwindCSS', 'https://cv.vn/tran-thi-bich.pdf', '2025-02-03', N'2 năm kinh nghiệm Frontend Developer'),
(3, N'Cử nhân Marketing - ĐH Kinh tế TP.HCM', N'SEO, SEM, Google Ads, Content Marketing', 'https://cv.vn/le-hoang-nam.pdf', '2025-02-05', N'3 năm kinh nghiệm Digital Marketing'),
(4, N'Cử nhân Kinh doanh Quốc tế - ĐH Ngoại thương', N'Logistics, Incoterms, Xuất nhập khẩu, Tiếng Anh', 'https://cv.vn/pham-minh-tuan.pdf', '2025-02-07', N'4 năm kinh nghiệm Xuất nhập khẩu'),
(5, N'Kỹ sư Cơ khí - ĐH Sư phạm Kỹ thuật', N'AutoCAD, SolidWorks, CNC, Gia công chế tạo', 'https://cv.vn/hoang-thu-trang.pdf', '2025-02-10', N'3 năm kinh nghiệm Kỹ sư Cơ khí');
GO

-- 5. Thêm dữ liệu bảng CATEGORIES (5 ngành nghề theo đúng yêu cầu)
INSERT INTO [dbo].[CATEGORIES] ([CategoryName]) VALUES
(N'Công nghệ thông tin'),
(N'Marketing'),
(N'Kinh doanh quốc tế'),
(N'Cơ khí'),
(N'Thiết kế đồ họa');
GO

-- 6. Thêm dữ liệu bảng JOBS (5 dòng chia theo tỷ lệ: CNTT 2, Marketing 1, KDQT 1, Cơ khí 1, Thiết kế đồ họa 0)
INSERT INTO [dbo].[JOBS] ([CompanyId], [Title], [Description], [SalaryRange], [Location], [Deadline], [Status], [PostedDate], [CategoryId]) VALUES
(1, N'Lập trình viên C# / ASP.NET MVC', N'Phát triển hệ thống web ứng dụng sử dụng C#, ASP.NET MVC, Entity Framework và SQL Server.', N'15 - 25 triệu VNĐ', N'TP. Hồ Chí Minh', '2026-12-30', N'Open', '2025-02-01', 1),
(2, N'Frontend Engineer (ReactJS/Next.js)', N'Xây dựng giao diện web hiệu năng cao cho hệ sinh thái ứng dụng người dùng.', N'20 - 35 triệu VNĐ', N'TP. Hồ Chí Minh', '2026-12-15', N'Open', '2025-02-05', 1),
(3, N'Chuyên viên Digital Marketing', N'Lên kế hoạch chiến dịch quảng cáo, tối ưu SEO/SEM và quản trị nội dung đa kênh.', N'12 - 20 triệu VNĐ', N'Hà Nội', '2026-12-10', N'Open', '2025-02-08', 2),
(4, N'Chuyên viên Xuất nhập khẩu & Logistics', N'Đàm phán hợp đồng thương mại quốc tế, xử lý chứng từ hải quan và chuỗi cung ứng.', N'18 - 30 triệu VNĐ', N'TP. Hồ Chí Minh', '2026-12-20', N'Open', '2025-02-10', 3),
(5, N'Kỹ sư Thiết kế Cơ khí Chế tạo máy', N'Thiết kế bản vẽ kỹ thuật 2D/3D trên AutoCAD, SolidWorks và giám sát gia công sản xuất.', N'16 - 28 triệu VNĐ', N'TP. Hồ Chí Minh', '2026-12-01', N'Open', '2025-02-12', 4);
GO

-- 7. Thêm dữ liệu bảng APPLICATIONS (5 đơn ứng tuyển mẫu)
INSERT INTO [dbo].[APPLICATIONS] ([JobId], [CandidateId], [CoverLetter], [AppliedDate], [Status], [UserId], [CV_Url]) VALUES
(1, 1, N'Mong muốn đóng góp kinh nghiệm lập trình C# ASP.NET MVC vào các dự án tại FPT Software.', '2025-02-15', N'Pending', 1, 'https://cv.vn/nguyen-van-an.pdf'),
(2, 2, N'Rất ấn tượng với các sản phẩm của VNG, tôi tự tin với kỹ năng ReactJS và tối ưu UI/UX.', '2025-02-16', N'Reviewed', 2, 'https://cv.vn/tran-thi-bich.pdf'),
(3, 3, N'Tôi có kinh nghiệm triển khai chiến dịch Digital Marketing và mong muốn gia nhập Viettel.', '2025-02-17', N'Interview', 3, 'https://cv.vn/le-hoang-nam.pdf'),
(4, 4, N'Với 4 năm kinh nghiệm Logistics và Ngoại thương, tôi hy vọng được đồng hành cùng công ty.', '2025-02-18', N'Accepted', 4, 'https://cv.vn/pham-minh-tuan.pdf'),
(5, 5, N'Thành thạo AutoCAD và SolidWorks, tôi mong muốn ứng tuyển vị trí Kỹ sư Cơ khí.', '2025-02-19', N'Pending', 5, 'https://cv.vn/hoang-thu-trang.pdf');
GO

-- =================================================================================
-- PHẦN 3: TRUY VẤN KIỂM TRA DỮ LIỆU
-- =================================================================================
SELECT * FROM [dbo].[ROLES];
SELECT * FROM [dbo].[COMPANIES];
SELECT * FROM [dbo].[USERS];
SELECT * FROM [dbo].[CANDIDATEPROFILES];
SELECT * FROM [dbo].[CATEGORIES];
SELECT * FROM [dbo].[JOBS];
SELECT * FROM [dbo].[APPLICATIONS];
GO