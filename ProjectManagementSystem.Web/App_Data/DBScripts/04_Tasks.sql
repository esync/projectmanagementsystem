CREATE TABLE [dbo].[Tasks]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TaskName] NVARCHAR(256) NOT NULL, 
    [ProjectId] INT NOT NULL, 
    [EmployeeId] INT NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [TaskStatus] NVARCHAR(50) NOT NULL, 
    [Comments] NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_Tasks_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]), 
    CONSTRAINT [FK_Tasks_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
)
