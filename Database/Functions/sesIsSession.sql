
CREATE FUNCTION [dbo].[sesIsSession]
(
	@Type char(6)
)
RETURNS bit
AS
BEGIN
/*
AF 20140902
*/

--return iif(@Type in ('SESSCF', 'SESSCO', 'SESSCS', 'SESSRQ'), 1, 0);
return iif(substring(@Type, 1, 4) = 'SESS', 1, 0);

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesIsSession] TO [websiterole]
    AS [dbo];

