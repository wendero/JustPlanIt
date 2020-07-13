using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JustPlanIt.Models;
using JustPlanIt.Classes;

namespace JustPlanIt.Controllers
{
    [ApiController]
    [Route("api/room/{roomId}/story")]
    public class StoryController : ControllerBase
    {
        private List<Room> Rooms
        {
            get
            {
                return RoomProvider.Rooms;
            }
        }

        private readonly ILogger<StoryController> _logger;

        public StoryController(ILogger<StoryController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public object AddStory(int roomId, [FromBody] StoryApiModel story)
        {
            if (string.IsNullOrWhiteSpace(story.Title))
            {
                return new BadRequestObjectResult(new { Message = "Title is not valid" });
            }

            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }

            var newStory = new Story()
            {
                Title = story.Title
            };

            room.Stories.Add(newStory);
            return Created("", newStory);
        }
        [HttpPatch("{storyId}")]
        public object ChangeStory(int roomId, int storyId, [FromBody]StoryApiModel storyModel)
        {
            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }

            var story = room.Stories.FirstOrDefault(w => w.Identifier == storyId);
            if (story == null)
            {
                return new NotFoundObjectResult(new { Message = "Story not found" });
            }

            story.Title = storyModel.Title ?? story.Title;
            story.Points = storyModel.Points ?? story.Points;
            story.Status = storyModel.Status ?? story.Status;

            return new AcceptedResult("", story);
        }
        [HttpGet("{storyId}")]
        public object GetStory(int roomId, int storyId)
        {
            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }

            var story = room.Stories.FirstOrDefault(w => w.Identifier == storyId);
            if (story == null)
            {
                return new NotFoundObjectResult(new { Message = "Story not found" });
            }
            return story;
        }

        [HttpDelete("{storyId}")]
        public object DeleteStory(int roomId, int storyId)
        {
            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }

            var story = room.Stories.FirstOrDefault(w => w.Identifier == storyId);
            if (story == null)
            {
                return new NotFoundObjectResult(new { Message = "Story not found" });

            }
            return room.Stories.Remove(story);
        }
        public class StoryApiModel
        {
            public string Title { get; set; }
            public StoryStatus? Status { get; set; }
            public int? Points { get; set; }
        }
    }
}
