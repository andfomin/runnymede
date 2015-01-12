

CREATE PROCEDURE [dbo].[copUpdatePriority]
	@UserId int,
	@ResourceId int,
	@Priority tinyint
AS
BEGIN
SET NOCOUNT ON;

-- We expect a dbo.libPostResourceView call has happend before this call and we assume the record does exist. 
update dbo.libUserResources 
set CopycatPriority = @Priority
where UserId = @UserId
	and ResourceId = @ResourceId;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[copUpdatePriority] TO [websiterole]
    AS [dbo];

