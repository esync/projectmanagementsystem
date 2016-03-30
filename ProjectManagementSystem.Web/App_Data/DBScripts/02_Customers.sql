CREATE TABLE [dbo].[Customers]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[CustomerName] NVARCHAR(50) NOT NULL, 
    [ContactPerson] NVARCHAR(50) NULL, 
	[ContactPhone] NVARCHAR(50) NULL,
    [UserId] NVARCHAR(128) NULL, 
    CONSTRAINT [FK_Customers_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]), 
)
