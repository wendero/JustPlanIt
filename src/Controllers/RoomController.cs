using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JustPlanIt.Models;
using JustPlanIt.Classes;
using System.Threading.Tasks;

namespace JustPlanIt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;

        public RoomController(ILogger<RoomController> logger)
        {
            _logger = logger;
        }

        private List<Room> Rooms
        {
            get
            {
                return RoomProvider.Rooms;
            }
        }

        [HttpGet("{roomId}")]
        public object Get(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room != null)
            {
                return room;
            }

            return new NotFoundObjectResult(new { Message = "This session no longer exists" });
        }
        [HttpGet("{roomId}/check")]
        public bool Check(int roomId)
        {
            return this.Rooms.Any(w => w.Identifier == roomId);
        }
        [HttpPost]
        public object Create(RoomApiModel body)
        {
            if (string.IsNullOrWhiteSpace(body.Room))
            {
                return new BadRequestObjectResult(new { Message = "Room name is not valid" });
            }

            if (string.IsNullOrWhiteSpace(body.Name))
            {
                return new BadRequestObjectResult(new { Message = "Member name is not valid" });
            }

            var room = new Room() { Name = body.Room };

            room.Members.Add(new Member() { Name = body.Name, Leader = true });
            this.Rooms.Add(room);

            return new CreatedResult($"room/{room.Identifier}", room);
        }
        [HttpPost("{roomId}/join")]
        public object Join(int roomId, [FromBody] RoomApiModel body)
        {
            if (string.IsNullOrWhiteSpace(body.Name))
            {
                return new BadRequestObjectResult(new { Message = "Member name is not valid" });
            }

            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }

            var member = new Member() { Name = body.Name };

            room.Members.Add(member);

            return new AcceptedResult(location: null, member);
        }
        [HttpDelete("{roomId}")]
        public object CloseRoom(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);
            if (room == null)
            {
                return new NotFoundObjectResult(new { Message = "This session no longer exists" });
            }
            room.IsClosing = true;

            CloseRoomAsync(room.Identifier);

            return room;
        }
        private async void CloseRoomAsync(int roomId)
        {
            await Task.Delay(10000);

            var room = this.Rooms.FirstOrDefault(w => w.Identifier == roomId);

            this.Rooms.Remove(room);
        }

        public class RoomApiModel
        {
            public string Room { get; set; }
            public string Name { get; set; }
        }
        public class ConfigApiModel
        {
            public Dictionary<string, object> Values { get; set; }
        }
    }
}
