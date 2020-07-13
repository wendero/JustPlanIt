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
    [Route("api/room/{roomId}/vote")]
    public class VoteController : ControllerBase
    {
        private List<Room> Rooms
        {
            get
            {
                return RoomProvider.Rooms;
            }
        }

        private readonly ILogger<StoryController> _logger;

        public VoteController(ILogger<StoryController> logger)
        {
            _logger = logger;
        }
        [HttpPost("{storyId}/start")]
        public object StartVoting(int roomId, int storyId)
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

            story.Votes.Clear();
            story.Status = StoryStatus.Voting;

            return story;
        }
        [HttpPost("stop")]
        public object StopVoting(int roomId, string storyId)
        {
            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }

            var story = room.Stories.FirstOrDefault(w => w.Status == StoryStatus.Voting);
            if (story == null)
            {
                return new NotFoundObjectResult(new { Message = "There is no running voting" });
            }

            if (!story.Votes.Any())
            {
                return new BadRequestObjectResult(new { Message = "Voting cannot be stopped once no one has voted yet" });
            }

            story.Status = StoryStatus.ShowingResults;

            var results = ReadResults(room, story);
            return results;
        }
        [HttpPost("{storyId}")]
        public object Vote(int roomId, int storyId, [FromBody] VoteApiModel voteModel)
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

            var member = room.Members.FirstOrDefault(w => w.Identifier == voteModel.MemberId);
            if (member == null)
            {
                return new NotFoundObjectResult(new { Message = "Member not found" });
            }

            if (!story.Votes.Any(w => w.Key == member.Identifier.ToString()))
            {
                story.Votes.Add(member.Identifier.ToString(), voteModel.Points);
            }
            else
            {
                story.Votes[member.Identifier.ToString()] = voteModel.Points;
            }

            return member;
        }

        [HttpGet("{storyId}/results")]
        public object ShowResults(int roomId, int storyId)
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

            story.Status = StoryStatus.ShowingResults;

            var results = ReadResults(room, story);
            return results;
        }
        private ResultsApiModel ReadResults(Room room, Story story)
        {
            var votes = story.Votes.Values;

            var max = votes.Max();
            var min = votes.Min();
            var average = votes.Average();

            var maxMemberIds = story.Votes.Where(w => w.Value == max).Select(s => Convert.ToInt32(s.Key));
            var minMemberIds = story.Votes.Where(w => w.Value == min).Select(s => Convert.ToInt32(s.Key));

            var maxMembers = room.Members.Where(w => maxMemberIds.Contains(w.Identifier)).ToList();
            var minMembers = room.Members.Where(w => minMemberIds.Contains(w.Identifier) && !maxMemberIds.Contains(w.Identifier)).ToList();
            var otherwiseMembers = room.Members.Where(w => !minMemberIds.Contains(w.Identifier) && !maxMemberIds.Contains(w.Identifier)).ToList();

            var results = new ResultsApiModel()
            {
                Story = story,
                Maximum = max,
                Minimum = min,
                Average = average,

                MaximumMembers = maxMembers,
                MinimumMembers = minMembers,
                OtherMembers = otherwiseMembers
            };

            return results;
        }


        public class StoryApiModel
        {
            public int Points { get; set; }
        }
        public class VoteApiModel
        {
            public int MemberId { get; set; }
            public int Points { get; set; } = 0;
        }
        public class ResultsApiModel
        {
            public Story Story { get; set; }
            public double Average { get; set; }
            public double Maximum { get; set; }
            public double Minimum { get; set; }
            public List<Member> MaximumMembers { get; set; }
            public List<Member> MinimumMembers { get; set; }
            public List<Member> OtherMembers { get; set; }
        }

    }
}
