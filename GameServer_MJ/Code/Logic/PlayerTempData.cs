using System;
using System.Collections.Generic;

namespace SocketTest
{
	public class PlayerTempData
	{
		public enum Status
		{
			None,
			Room,
			Fight,
		}
		
		public Status status;
		public Room room;
		public int team = 1;
		public bool isOwner = false;
		
		public PlayerTempData()
		{
			status = Status.None;
		}
	}
}
