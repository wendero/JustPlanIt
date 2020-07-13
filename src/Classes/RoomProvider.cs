using System.Collections.Generic;
using JustPlanIt.Models;

namespace JustPlanIt.Classes
{
    public class RoomProvider
    {
        private static readonly List<Room> _rooms = new List<Room>();

        public static List<Room> Rooms
        {
            get
            {
                return _rooms;
            }
        }
    }
}