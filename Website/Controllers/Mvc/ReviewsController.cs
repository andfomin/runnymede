using Dapper;
using Runnymede.Common.Models;
using Runnymede.Website.Models;
using Runnymede.Common.Utils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Runnymede.Website.Utils;

namespace Runnymede.Website.Controllers.Mvc
{
    public class ReviewsController : Runnymede.Website.Utils.CustomController
    {

        // GET: /reviews/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /reviews/edit/12345 by ReviewId
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var exercise = (await ExerciseUtils.GetExerciseWithReviews("exeGetReview", new { UserId = this.GetUserId(), Id = id, })).Single();

            if (exercise.CardId.HasValue)
            {
                ViewBag.cardParam = await ExerciseUtils.GetCardWithItems(exercise.CardId.Value);
            }

            ViewBag.ExerciseParam = exercise;

            switch (exercise.ArtifactType)
            {
                case ArtifactType.Jpeg:
                    return View("EditWriting");
                case ArtifactType.Mp3:
                    return View("EditRecording");
                default:
                    return HttpNotFound(exercise.ArtifactType);
            }
        }

    }
}