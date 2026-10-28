-- =================================================================================
-- SQL SERVER (T-SQL) (7 BẢNG)
-- =================================================================================

USE master;
GO

-- Xóa Database cũ nếu đã tồn tại để tránh lỗi khi bấm F5 nhiều lần
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RecruitmentDB')
BEGIN
    ALTER DATABASE RecruitmentDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RecruitmentDB;
END
GO

-- 1. Tạo mới Database
CREATE DATABASE RecruitmentDB;
GO

USE RecruitmentDB;
GO
-- =================================================================================
-- PHẦN 1: KHỞI TẠO 7 BẢNG VỚI ĐẦY ĐỦ RÀNG BUỘC (PK, FK, UNIQUE, CHECK, DEFAULT)
-- =================================================================================

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
    [Logo_Url]    VARCHAR(255)  NOT NULL,
    [Address]     NVARCHAR(255) NOT NULL,
    [Website]     VARCHAR(255)  NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_COMPANIES] PRIMARY KEY CLUSTERED ([CompanyId] ASC),
    CONSTRAINT [UQ_COMPANIES_CompanyName] UNIQUE ([CompanyName]),
    CONSTRAINT [CK_COMPANIES_CompanyId] CHECK ([CompanyId] > 0),
    CONSTRAINT [CK_COMPANIES_LogoUrl] CHECK ([Logo_Url] LIKE 'http://%' OR [Logo_Url] LIKE 'https://%'),
    CONSTRAINT [CK_COMPANIES_Website] CHECK ([Website] LIKE 'http://%' OR [Website] LIKE 'https://%')
);
GO

-- 3. Bảng USERS (Tài khoản người dùng / Ứng viên)
-- Lưu ý chuẩn hóa: CompanyId cho phép NULL để hỗ trợ ứng viên tự do không thuộc công ty nào
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

-- Thiết lập khóa ngoại từ COMPANIES(UserId) tham chiếu tới USERS(UserId)
-- Tạm thời NOCHECK khi chưa có dữ liệu để tránh khóa vòng (Circular FK), sẽ bật lại ngay sau khi INSERT
ALTER TABLE [dbo].[COMPANIES] WITH NOCHECK
ADD CONSTRAINT [FK_COMPANIES_USERS] FOREIGN KEY ([UserId])
    REFERENCES [dbo].[USERS]([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [dbo].[COMPANIES] NOCHECK CONSTRAINT [FK_COMPANIES_USERS];
GO

-- 4. Bảng CANDIDATEPROFILES (Hồ sơ chi tiết của ứng viên - Quan hệ 1-1 với USERS)
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
    CONSTRAINT [CK_CANDIDATEPROFILES_CVUrl] CHECK ([CV_Url] LIKE 'http://%' OR [CV_Url] LIKE 'https://%'),
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

-- 7. Bảng APPLICATIONS (Đơn ứng tuyển của ứng viên vào vị trí công việc)
CREATE TABLE [dbo].[APPLICATIONS]
(
    [ApplicationId] INT IDENTITY(1,1) NOT NULL,
    [JobId]         INT           NOT NULL,
    [CandidateId]   INT           NOT NULL,
    [CoverLetter]   NVARCHAR(MAX) NOT NULL,
    [AppliedDate]   DATE          NOT NULL CONSTRAINT [DF_APPLICATIONS_AppliedDate] DEFAULT (CAST(GETDATE() AS DATE)),
    [Status]        NVARCHAR(50)  NOT NULL CONSTRAINT [DF_APPLICATIONS_Status] DEFAULT (N'Pending'),
    [UserId]        INT           NOT NULL,
    CONSTRAINT [PK_APPLICATIONS] PRIMARY KEY CLUSTERED ([ApplicationId] ASC),
    CONSTRAINT [UQ_APPLICATIONS_Job_User] UNIQUE ([JobId], [UserId]),
    CONSTRAINT [CK_APPLICATIONS_ApplicationId] CHECK ([ApplicationId] > 0),
    CONSTRAINT [CK_APPLICATIONS_Status] CHECK ([Status] IN (N'Pending', N'Reviewed', N'Interview', N'Accepted', N'Rejected')),
    CONSTRAINT [CK_APPLICATIONS_Candidate_User_Sync] CHECK ([CandidateId] = [UserId]),
    CONSTRAINT [FK_APPLICATIONS_JOBS] FOREIGN KEY ([JobId])
        REFERENCES [dbo].[JOBS]([JobId]) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT [FK_APPLICATIONS_USERS] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[USERS]([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT [FK_APPLICATIONS_CANDIDATEPROFILES] FOREIGN KEY ([CandidateId])
        REFERENCES [dbo].[CANDIDATEPROFILES]([ProfileId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

-- =================================================================================
-- PHẦN 2: THÊM DỮ LIỆU MẪU GIỮ NGUYÊN 5 BẢN GHI / BẢNG (35 BẢN GHI TỔNG CỘNG)
-- =================================================================================

-- 1. Thêm dữ liệu bảng ROLES (5 dòng)
INSERT INTO [dbo].[ROLES] ([RoleName]) VALUES
(N'Admin - Quản trị viên'),
(N'Employer - Nhà tuyển dụng'),
(N'Candidate - Ứng viên'),
(N'HR Manager - Quản lý nhân sự'),
(N'Moderator - Kiểm duyệt viên');
GO

-- 2. Thêm dữ liệu bảng COMPANIES (5 dòng)
INSERT INTO [dbo].[COMPANIES] ([UserId], [CompanyName], [Logo_Url], [Address], [Website], [Description]) VALUES
(1, N'FPT Software', 'https://fptsoftware.com/logo.png', N'Khu Công nghệ cao Hòa Lạc, Hà Nội', 'https://fptsoftware.com', N'Công ty xuất khẩu phần mềm và chuyển đổi số hàng đầu Việt Nam.'),
(2, N'VNG Corporation', 'https://vng.com.vn/logo.png', N'Z06 Đường số 13, Quận 7, TP.HCM', 'https://vng.com.vn', N'Tập đoàn công nghệ kỳ lân chuyên về Zalo, AI, Cloud và trò chơi trực tuyến.'),
(3, N'Viettel Solutions', 'https://solutions.viettel.vn/logo.png', N'Số 1 Trần Hữu Dực, Nam Từ Liêm, Hà Nội', 'https://solutions.viettel.vn', N'Tổng công ty Giải pháp Doanh nghiệp Viettel, tiên phong chuyển đổi số.'),
(4, N'MoMo (M_Service)', 'https://momo.vn/logo.png', N'Phú Mỹ Hưng Tower, Quận 7, TP.HCM', 'https://momo.vn', N'Siêu ứng dụng tài chính và thanh toán điện tử hàng đầu Việt Nam.'),
(5, N'Tiki Corporation', 'https://tiki.vn/logo.png', N'52 Út Tịch, Quận Tân Bình, TP.HCM', 'https://tiki.vn', N'Nền tảng thương mại điện tử và chuỗi cung ứng kho vận thông minh.');
GO

-- 3. Thêm dữ liệu bảng USERS (5 dòng)
INSERT INTO [dbo].[USERS] ([Email], [PasswordHash], [FullName], [Phone], [RoleId], [IsActive], [CreatedAt], [CompanyId]) VALUES
('nguyen.van.an@gmail.com', '$2b$12$HashPass001', N'Nguyễn Văn An', '0901234567', 3, 1, '2025-01-10', 1),
('tran.thi.bich@gmail.com', '$2b$12$HashPass002', N'Trần Thị Bích', '0912345678', 3, 1, '2025-01-12', 2),
('le.hoang.nam@gmail.com', '$2b$12$HashPass003', N'Lê Hoàng Nam', '0987654321', 3, 1, '2025-01-15', 3),
('pham.minh.tuan@gmail.com', '$2b$12$HashPass004', N'Phạm Minh Tuấn', '0933445566', 2, 1, '2025-01-18', 4),
('hoang.thu.trang@gmail.com', '$2b$12$HashPass005', N'Hoàng Thu Trang', '0977889900', 3, 1, '2025-01-20', 5);
GO

-- Kích hoạt và kiểm tra toàn vẹn khóa ngoại FK_COMPANIES_USERS sau khi cả 2 bảng đã có dữ liệu
ALTER TABLE [dbo].[COMPANIES] WITH CHECK CHECK CONSTRAINT [FK_COMPANIES_USERS];
GO

-- 4. Thêm dữ liệu bảng CANDIDATEPROFILES (5 dòng)
INSERT INTO [dbo].[CANDIDATEPROFILES] ([UserId], [Education], [Skills], [CV_Url], [UpdatedAt], [Experience]) VALUES
(1, N'Cử nhân CNTT - ĐH Bách Khoa Hà Nội', N'Java, Spring Boot, Microservices, SQL Server', 'https://cv.vn/nguyen-van-an.pdf', '2025-02-01', N'3 năm kinh nghiệm Backend Developer'),
(2, N'Cử nhân KHMT - ĐH KHTN TP.HCM', N'ReactJS, TypeScript, Next.js, TailwindCSS', 'https://cv.vn/tran-thi-bich.pdf', '2025-02-03', N'2 năm kinh nghiệm Frontend Developer'),
(3, N'Kỹ sư HTTT - ĐH Công nghệ ĐHQGHN', N'Python, PyTorch, SQL, Spark, Data Engineering', 'https://cv.vn/le-hoang-nam.pdf', '2025-02-05', N'4 năm kinh nghiệm AI / Data Engineer'),
(4, N'Cử nhân QTKD - ĐH Kinh tế Quốc dân', N'Agile/Scrum, UML, BPMN, Jira, Figma', 'https://cv.vn/pham-minh-tuan.pdf', '2025-02-07', N'5 năm kinh nghiệm Business Analyst & PO'),
(5, N'Kỹ sư Phần mềm - ĐH FPT', N'Docker, Kubernetes, AWS, CI/CD, Linux', 'https://cv.vn/hoang-thu-trang.pdf', '2025-02-10', N'3 năm kinh nghiệm DevOps Engineer');
GO

-- 5. Thêm dữ liệu bảng CATEGORIES (5 dòng)
INSERT INTO [dbo].[CATEGORIES] ([CategoryName]) VALUES
(N'Phát triển phần mềm (Software Development)'),
(N'Khoa học dữ liệu & AI (Data Science & AI)'),
(N'Hạ tầng & Điện toán đám mây (DevOps & Cloud)'),
(N'Phân tích nghiệp vụ & Sản phẩm (BA / Product)'),
(N'Kiểm thử phần mềm (QA / QC Testing)');
GO

-- 6. Thêm dữ liệu bảng JOBS (5 dòng)
INSERT INTO [dbo].[JOBS] ([CompanyId], [Title], [Description], [SalaryRange], [Location], [Deadline], [Status], [PostedDate], [CategoryId]) VALUES
(1, N'Senior Java Backend Developer', N'Phát triển hệ thống Core Banking và Microservices cho khách hàng toàn cầu.', N'25 - 40 triệu VNĐ', N'Hà Nội', '2025-04-30', N'Open', '2025-02-01', 1),
(2, N'Frontend Engineer (ReactJS/Next.js)', N'Xây dựng giao diện web hiệu năng cao cho hệ sinh thái ứng dụng người dùng.', N'20 - 35 triệu VNĐ', N'TP. Hồ Chí Minh', '2025-04-15', N'Open', '2025-02-05', 1),
(3, N'AI / Machine Learning Engineer', N'Nghiên cứu và triển khai các mô hình NLP và Computer Vision.', N'30 - 50 triệu VNĐ', N'Hà Nội', '2025-05-10', N'Open', '2025-02-08', 2),
(4, N'Senior Business Analyst (Fintech)', N'Phân tích nghiệp vụ ví điện tử, viết tài liệu SRS/BRD và quản lý backlog.', N'22 - 38 triệu VNĐ', N'TP. Hồ Chí Minh', '2025-04-20', N'Open', '2025-02-10', 4),
(5, N'DevOps / Cloud Engineer (AWS/K8s)', N'Vận hành hạ tầng Kubernetes, tối ưu CI/CD pipeline và giám sát hệ thống.', N'28 - 45 triệu VNĐ', N'TP. Hồ Chí Minh', '2025-05-01', N'Open', '2025-02-12', 3);
GO

-- 7. Thêm dữ liệu bảng APPLICATIONS (5 dòng)
INSERT INTO [dbo].[APPLICATIONS] ([JobId], [CandidateId], [CoverLetter], [AppliedDate], [Status], [UserId]) VALUES
(1, 1, N'Mong muốn đóng góp 3 năm kinh nghiệm Java Spring Boot vào các dự án quốc tế tại FPT Software.', '2025-02-15', N'Pending', 1),
(2, 2, N'Rất ấn tượng với các sản phẩm của VNG, tôi tự tin với kỹ năng ReactJS và tối ưu UI/UX.', '2025-02-16', N'Reviewed', 2),
(3, 3, N'Tôi có kinh nghiệm thực chiến triển khai mô hình AI/NLP và mong muốn gia nhập Viettel Solutions.', '2025-02-17', N'Interview', 3),
(4, 4, N'Với 5 năm kinh nghiệm BA/PO mảng Fintech, tôi hy vọng được đồng hành cùng siêu ứng dụng MoMo.', '2025-02-18', N'Accepted', 4),
(5, 5, N'Đam mê tự động hóa hạ tầng Cloud và K8s, tôi mong muốn ứng tuyển vị trí DevOps tại Tiki.', '2025-02-19', N'Pending', 5);
GO

-- =================================================================================
-- PHẦN 3: CÂU LỆNH SELECT 
-- =================================================================================
USE [RecruitmentDB];
GO

-- 1. Xem danh sách Vai trò người dùng (Bảng ROLES)
SELECT * FROM [dbo].[ROLES];

-- 2. Xem danh sách Công ty tuyển dụng (Bảng COMPANIES)
SELECT * FROM [dbo].[COMPANIES];

-- 3. Xem danh sách Tài khoản người dùng / Ứng viên (Bảng USERS)
SELECT * FROM [dbo].[USERS];

-- 4. Xem danh sách Hồ sơ chi tiết ứng viên (Bảng CANDIDATEPROFILES)
SELECT * FROM [dbo].[CANDIDATEPROFILES];

-- 5. Xem danh sách Danh mục ngành nghề (Bảng CATEGORIES)
SELECT * FROM [dbo].[CATEGORIES];

-- 6. Xem danh sách Tin tuyển dụng việc làm (Bảng JOBS)
SELECT * FROM [dbo].[JOBS];

-- 7. Xem danh sách Đơn ứng tuyển (Bảng APPLICATIONS)
SELECT * FROM [dbo].[APPLICATIONS];
GO