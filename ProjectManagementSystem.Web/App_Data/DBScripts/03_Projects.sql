CREATE TABLE [dbo].[Projects]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ProjectName] NVARCHAR(256) NOT NULL, 
    [CustomerId] INT NOT NULL, 
    [EmployeeId] INT NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [Comments] NVARCHAR(MAX) NULL, 
    [Attachment] VARBINARY(MAX) NULL, 
    CONSTRAINT [FK_Projects_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([Id]), 
    CONSTRAINT [FK_Projects_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
)
