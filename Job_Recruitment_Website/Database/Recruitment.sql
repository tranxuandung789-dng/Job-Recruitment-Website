USE master;
GO



-- 1. Tạo mới Database
CREATE DATABASE RecruitmentDB;
GO

USE RecruitmentDB;
GO

-- =================================================================
-- TẠO CẤU TRÚC CÁC BẢNG (TABLES) CHUẨN SQL SERVER CHO ENTITY FRAMEWORK
-- =================================================================

-- 2. Bảng users (Người dùng)
CREATE TABLE dbo.users (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name NVARCHAR(100) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('candidate', 'employer')),
    phone VARCHAR(20) NULL,
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- 3. Bảng cv_profiles (Hồ sơ / CV)
CREATE TABLE dbo.cv_profiles (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    user_id INT NOT NULL,
    title NVARCHAR(150) NOT NULL,
    summary NVARCHAR(MAX) NULL,
    skills NVARCHAR(MAX) NULL,
    experience NVARCHAR(MAX) NULL,
    education NVARCHAR(MAX) NULL,
    resume_url VARCHAR(255) NULL,
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_cv_profiles_users FOREIGN KEY (user_id) REFERENCES dbo.users(id)
);
GO

-- 4. Bảng jobs (Tin tuyển dụng)
CREATE TABLE dbo.jobs (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    posted_by_user_id INT NOT NULL,
    title NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    location NVARCHAR(150) NULL,
    salary_range NVARCHAR(100) NULL,
    job_type NVARCHAR(50) NULL,
    status NVARCHAR(50) DEFAULT N'Đang mở',
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_jobs_users FOREIGN KEY (posted_by_user_id) REFERENCES dbo.users(id)
);
GO

-- 5. Bảng applications (Đơn ứng tuyển)
CREATE TABLE dbo.applications (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    job_id INT NOT NULL,
    cv_id INT NOT NULL,
    applied_at DATETIME DEFAULT GETDATE(),
    status NVARCHAR(50) DEFAULT N'Đang xem xét' CHECK (status IN (N'Đang xem xét', N'Từ chối', N'Nhận')),
    cover_letter NVARCHAR(MAX) NULL,
    CONSTRAINT FK_applications_jobs FOREIGN KEY (job_id) REFERENCES dbo.jobs(id),
    CONSTRAINT FK_applications_cv_profiles FOREIGN KEY (cv_id) REFERENCES dbo.cv_profiles(id)
);
GO

-- =================================================================
-- THÊM 5 DỮ LIỆU MẪU CHO MỖI BẢNG (CÓ N'...' ĐỂ KHÔNG LỖI FONT)
-- =================================================================

-- 1. Thêm 5 dòng vào bảng users (3 ứng viên, 2 nhà tuyển dụng)
INSERT INTO dbo.users (username, email, password_hash, full_name, role, phone) VALUES
('ungvien_an', 'an.nguyen@email.com', '123456', N'Nguyễn Văn An', 'candidate', '0901234567'),
('ungvien_binh', 'binh.tran@email.com', '123456', N'Trần Thị Bình', 'candidate', '0902345678'),
('ungvien_cuong', 'cuong.le@email.com', '123456', N'Lê Hoàng Cường', 'candidate', '0903456789'),
('hr_fpt', 'tuyendung@fpt.com.vn', '123456', N'Phòng Nhân Sự FPT', 'employer', '02873001111'),
('hr_vng', 'careers@vng.com.vn', '123456', N'Tuyển Dụng VNG', 'employer', '02839623888');
GO

-- 2. Thêm 5 dòng vào bảng cv_profiles (user_id: 1, 2, 3 là ứng viên)
INSERT INTO dbo.cv_profiles (user_id, title, summary, skills, experience, education, resume_url) VALUES
(1, N'.NET Developer (WinForms / ASP.NET)', N'Sinh viên CNTT nắm vững lập trình hướng đối tượng và C#.', N'C#, .NET Framework, WinForms, SQL Server, Entity Framework', N'Thực tập sinh 3 tháng tại công ty phần mềm', N'Đại học chuyên ngành CNTT', 'cv_an_dotnet.pdf'),
(1, N'Backend Developer Intern', N'Đam mê phát triển hệ thống cơ sở dữ liệu và tối ưu truy vấn.', N'SQL Server, MySQL, C#, RESTful API', N'Đồ án môn học: Quản lý tiến trình và nhân sự bằng WinForms', N'Đại học chuyên ngành CNTT', 'cv_an_backend.pdf'),
(2, N'Frontend Web Developer', N'Xây dựng giao diện web tương tác và chuẩn responsive.', N'HTML, CSS, JavaScript, Bootstrap', N'1 năm làm Freelancer cắt giao diện web', N'Đại học chuyên ngành Khoa học Máy tính', 'cv_binh_frontend.pdf'),
(3, N'Fullstack Web Developer', N'Kinh nghiệm xây dựng ứng dụng web theo mô hình MVC.', N'ASP.NET MVC, C#, HTML/CSS/JS, Entity Framework', N'Tham gia 2 dự án web bán hàng tại trường', N'Đại học chuyên ngành Kỹ thuật phần mềm', 'cv_cuong_fullstack.pdf'),
(2, N'UI/UX & Frontend', N'Kết hợp tư duy thiết kế và kỹ năng lập trình giao diện.', N'Figma, HTML5, CSS3, JavaScript', N'Thiết kế giao diện cho 5 dự án sinh viên', N'Đại học chuyên ngành Khoa học Máy tính', 'cv_binh_uiux.pdf');
GO

-- 3. Thêm 5 dòng vào bảng jobs (posted_by_user_id: 4, 5 là nhà tuyển dụng)
INSERT INTO dbo.jobs (posted_by_user_id, title, description, location, salary_range, job_type, status) VALUES
(4, N'Lập trình viên C# .NET (Fresher)', N'Phát triển ứng dụng WinForms và Web trên nền tảng .NET và SQL Server.', N'TP. Thủ Đức, TP.HCM', N'10 - 15 triệu', N'Toàn thời gian', N'Đang mở'),
(4, N'Thực tập sinh ASP.NET MVC', N'Hỗ trợ đội ngũ phát triển xây dựng các module web quản lý nội bộ.', N'Bình Thạnh, TP.HCM', N'3 - 5 triệu', N'Bán thời gian', N'Đang mở'),
(5, N'Frontend Developer (HTML/CSS/JS)', N'Phát triển giao diện người dùng cho cổng thông tin khách hàng.', N'Quận 7, TP.HCM', N'12 - 18 triệu', N'Toàn thời gian', N'Đang mở'),
(5, N'Database Developer Intern', N'Hỗ trợ thiết kế CSDL, viết truy vấn và quản trị SQL Server.', N'Quận 7, TP.HCM', N'4 - 6 triệu', N'Bán thời gian', N'Đang mở'),
(4, N'Kỹ sư phần mềm Fullstack .NET', N'Phát triển từ cơ sở dữ liệu SQL Server đến giao diện người dùng.', N'Quận 1, TP.HCM', N'15 - 22 triệu', N'Toàn thời gian', N'Đang mở');
GO

-- 4. Thêm 5 dòng vào bảng applications
INSERT INTO dbo.applications (job_id, cv_id, status, cover_letter) VALUES
(1, 1, N'Đang xem xét', N'Em mong muốn áp dụng kiến thức C#, WinForms và Entity Framework vào dự án thực tế.'),
(2, 4, N'Nhận', N'Em xin gửi CV ứng tuyển vị trí thực tập sinh ASP.NET MVC.'),
(3, 3, N'Đang xem xét', N'Với nền tảng vững về HTML, CSS, JS, em tin mình phù hợp với vị trí này.'),
(4, 2, N'Từ chối', N'Em rất yêu thích mảng thiết kế cơ sở dữ liệu SQL Server và mong có cơ hội thực tập.'),
(5, 1, N'Đang xem xét', N'Em đã làm nhiều đồ án với C# và SQL Server, hy vọng được trao đổi thêm khi phỏng vấn.');
GO

-- Kiểm tra lại dữ liệu đã vào đủ chưa
SELECT * FROM dbo.users;
SELECT * FROM dbo.cv_profiles;
SELECT * FROM dbo.jobs;
SELECT * FROM dbo.applications;