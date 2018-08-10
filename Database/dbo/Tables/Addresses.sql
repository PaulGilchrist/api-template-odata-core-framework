CREATE TABLE [dbo].[Addresses] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [StreetNumber] INT            NOT NULL,
    [StreetName]   NVARCHAR (100) NOT NULL,
    [City]         NVARCHAR (20) NOT NULL,
    [State]        NVARCHAR (20) NOT NULL,
    [ZipCode]      NVARCHAR (10) NOT NULL,
    [Name]         NVARCHAR (50) NULL,
    [Suite]        NVARCHAR (10) NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([Id] ASC)
);

