using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Runnymede.Website.Models;
using System.Threading.Tasks;
using Runnymede.Common.Utils;

namespace Runnymede.Website.Controllers.Hubs
{
    [Authorize]
    public class ReviewHub : Hub
    {
        public override Task OnConnected()
        {
            var userId = AccountUtils.GetUserId(Context.User.Identity);
            Groups.Add(Context.ConnectionId, KeyUtils.IntToKey(userId));
            return base.OnConnected();
        }

        public async Task RegisterWatcher(int exerciseId)
        {
            var userId = AccountUtils.GetUserId(Context.User.Identity);
            const string sql = @"select UserId from dbo.exeExercises where Id = @ExerciseId;";
            var authorUserId = (await DapperHelper.QueryResilientlyAsync<int>(sql, new { ExerciseId = exerciseId, })).Single();
            if (userId == authorUserId)
            {
                var groupName = KeyUtils.IntToKey(exerciseId); // Corresponds to PieceTypes.PartitionKey
                await Groups.Add(Context.ConnectionId, groupName);
            }
            /* You should not manually remove the connection from the group when the user disconnects. This action is automatically performed by the SignalR framework. */
        }

    }
}