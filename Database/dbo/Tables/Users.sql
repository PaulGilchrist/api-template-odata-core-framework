CREATE TABLE [dbo].[Users] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [FirstName] NVARCHAR (20) NOT NULL,
    [LastName]  NVARCHAR (20) NOT NULL,
    [Email]     NVARCHAR (50) NULL,
    [Phone]     NVARCHAR (15) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

