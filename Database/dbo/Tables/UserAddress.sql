CREATE TABLE [dbo].[UserAddress] (
    [Users_Id]     INT NOT NULL,
    [Addresses_Id] INT NOT NULL,
    CONSTRAINT [PK_UserAddress] PRIMARY KEY CLUSTERED ([Users_Id] ASC, [Addresses_Id] ASC),
    CONSTRAINT [FK_UserAddress_Address] FOREIGN KEY ([Addresses_Id]) REFERENCES [dbo].[Addresses] ([Id]),
    CONSTRAINT [FK_UserAddress_User] FOREIGN KEY ([Users_Id]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_UserAddress_Address]
    ON [dbo].[UserAddress]([Addresses_Id] ASC);

