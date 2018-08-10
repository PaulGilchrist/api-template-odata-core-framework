CREATE TABLE [dbo].[Images]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [Url] NVARCHAR (MAX) NOT NULL,
    [SourceSystem] NVARCHAR (MAX) NOT NULL,
    [SourceId] INT NULL,
    [SortOrder] INT NOT NULL,
    CONSTRAINT [PK_Images] PRIMARY KEY CLUSTERED ([Id] ASC)
);
