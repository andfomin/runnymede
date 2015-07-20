

CREATE VIEW [dbo].[exeCardsWithItems]
AS
-- CI.CardId is used as a divider to split rows on Card and CardItem entities in Runnymede.Common.Utils.ExerciseUtils.GetCards()
select C.Id, C.[Type], C.Title, CI.CardId, CI.Position, CI.Contents
from dbo.exeCards C
	inner join dbo.exeCardItems as CI on C.Id = CI.CardId
GO
GRANT SELECT
    ON OBJECT::[dbo].[exeCardsWithItems] TO [websiterole]
    AS [dbo];

