

CREATE FUNCTION [dbo].[relGetSkypeDirectory]
(
)
/*
Returns rows in random order.
*/
RETURNS 
	@t TABLE 
	(
		RowNumber int identity primary key clustered,
		UserId int,
		Skype nvarchar(100),
		TimeBegin datetime2(7),
		TimeEnd datetime2(7),
		Announcement nvarchar(200)
	)
AS
BEGIN

	declare @Now datetime2(7) = sysutcdatetime();
	declare @Future datetime2(7) = dateadd(day, 1, @Now);
	declare @Shift float = sin(checksum(cast(@Now as nvarchar(100))));

/* +http://blogs.msdn.com/b/sqltips/archive/2005/07/20/441053.aspx
 INSERT queries that use SELECT with ORDER BY to populate rows guarantees how identity values are computed but not the order in which the rows are inserted
*/
	insert @t (UserId, Skype, TimeBegin, TimeEnd, Announcement)
		select UserId, Skype, TimeBegin, TimeEnd, Announcement 
		from dbo.relSkypeDirectory
		where TimeBegin <= @Now and @Now < coalesce(TimeEnd, @Future)
		order by checksum(cast((sin(Id) + @Shift) as nvarchar(100)));
	
	RETURN 
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[relGetSkypeDirectory] TO [websiterole]
    AS [dbo];

