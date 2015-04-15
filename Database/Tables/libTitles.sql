CREATE TABLE [dbo].[libTitles] (
    [Id]    AS             (isnull(CONVERT([bigint],(4294967296.))*binary_checksum([Title])+binary_checksum(reverse([Title])),(0))) PERSISTED NOT NULL,
    [Title] NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_libTitles] PRIMARY KEY CLUSTERED ([Id] ASC)
);







