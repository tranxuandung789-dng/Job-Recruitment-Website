USE RecruitmentDB;
GO

-- Tìm và xóa tất cả các ràng buộc CHECK trên bảng COMPANIES để không bị lỗi nữa
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'ALTER TABLE [dbo].[COMPANIES] DROP CONSTRAINT [' + name + N'];'
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID(N'[dbo].[COMPANIES]');

EXEC sp_executesql @sql;
GO