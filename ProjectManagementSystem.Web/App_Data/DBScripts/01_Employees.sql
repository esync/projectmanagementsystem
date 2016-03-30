CREATE TABLE [dbo].[Employees]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[EmployeeName] NVARCHAR(50) NOT NULL, 
    [Department] NVARCHAR(50) NOT NULL, 
    [UserId] NVARCHAR(128) NULL, 
    CONSTRAINT [FK_Employees_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]), 
)