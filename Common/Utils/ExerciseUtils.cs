using Runnymede.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Runnymede.Common.Utils
{
    public class ExerciseUtils
    {

        public static async Task<IEnumerable<ExerciseDto>> GetExerciseWithReviews(string sql, object param)
        {
            using (var connection = await DapperHelper.GetOpenConnectionAsync())
            {
                return await connection.QueryAsync<ExerciseDto, ReviewDto, ExerciseDto>(
                    sql,
                    (e, r) => { e.Reviews = new[] { r }; return e; },
                    param,
                    splitOn: "ExerciseId",
                    commandType: CommandType.StoredProcedure
                    );
            }
        }

        public static async Task<IEnumerable<CardDto>> GetCardsWithItems(string sql, object param)
        {
            IEnumerable<CardDto> cards;

            using (var conn = await DapperHelper.GetOpenConnectionAsync())
            {
                cards = await conn.QueryAsync<CardDto, CardItemDto, CardDto>(
                     sql,
                     (c, ci) => { c.Items = new[] { ci }; return c; },
                     param,
                     splitOn: "CardId",
                     commandType: CommandType.Text
                     );
            }

            cards = cards
                .GroupBy(i => i.Id)
                .Select(i =>
                {
                    var items = i
                            .SelectMany(j => j.Items)
                            .OrderBy(j => j.Position)
                            .ToList();
                    var c = i.First();
                    c.Items = items;
                    return c;
                })
                .OrderBy(i => Guid.NewGuid())
                ;

            return cards;
        }

        public static async Task<CardDto> GetCardWithItems(Guid cardId)
        {
            const string sql = @"
select Id, Title, CardId, Position, Contents
from dbo.exeCardsWithItems
where Id = @Id;
";
            return (await ExerciseUtils.GetCardsWithItems(sql, new { Id = cardId })).FirstOrDefault();
        }


    }
}
