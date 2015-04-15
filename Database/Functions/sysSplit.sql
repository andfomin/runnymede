CREATE FUNCTION [dbo].[sysSplit] (@Separator nchar(1), @CSV nvarchar(4000))
RETURNS table
AS
-- Split CSV. Author: Arnold Fribble. +http://www.sqlteam.com/forums/topic.asp?TOPIC_ID=50648
-- The separator does not have to be comma. If we split on space, a preceding, trailing, or an extra space in a malformed input string produces an empty string row.
RETURN (
    with Pieces(pn, [start], [stop]) as (
      select 1, 1, charindex(@Separator, @CSV)
      union all
      select pn + 1, [stop] + 1, charindex(@Separator, @CSV, [stop] + 1)
      from Pieces
      where [stop] > 0
    )
    select pn, substring(@CSV, [start], case when [stop] > 0 then [stop] - [start] else 4000 end) as S
    from Pieces
  )