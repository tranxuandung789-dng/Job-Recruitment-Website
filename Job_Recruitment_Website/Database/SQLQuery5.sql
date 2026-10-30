USE RecruitmentDB;
GO

UPDATE [dbo].[ROLES] SET RoleName = 'Admin' WHERE RoleId = 1;
UPDATE [dbo].[ROLES] SET RoleName = 'Recruiter' WHERE RoleId = 2;
UPDATE [dbo].[ROLES] SET RoleName = 'Candidate' WHERE RoleId = 3;