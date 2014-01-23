CREATE TYPE [dbo].[appUsersType] AS TABLE (
    [UserId] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC));




GO
GRANT EXECUTE
    ON TYPE::[dbo].[appUsersType] TO [websiterole]
    AS [dbo];

