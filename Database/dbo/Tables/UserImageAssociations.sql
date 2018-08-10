CREATE TABLE [dbo].[UserImageAssociations] (
    [UserId]  INT NOT NULL,
    [ImageId] INT NOT NULL,
    CONSTRAINT [PK_UserImageAssociations] PRIMARY KEY CLUSTERED ([UserId] ASC, [ImageId] ASC),
    CONSTRAINT [FK_UserImageAssociations_Image] FOREIGN KEY ([ImageId]) REFERENCES [dbo].[Images] ([Id]),
    CONSTRAINT [FK_UserImageAssociations_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_UserImage_Image]
    ON [dbo].[UserImageAssociations]([UserId] ASC);

