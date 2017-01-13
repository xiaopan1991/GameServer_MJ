using System;
using System.Collections.Generic;
using System.Linq;
using CommonDLL;
using LitJson;

namespace GameServer_MJ
{
	public class RoomOptions
	{
		public string RoomName = string.Empty;

		public RoomOptions()
		{
			this.RoomName = string.Empty;
		}
		public RoomOptions(string name)
		{
			this.RoomName = name;
		}
	}
	public class Room
	{
		public enum Status
		{
			Prepare = 1,
			Fight =2,
		}

		public Status status = Status.Prepare;
		public int maxPlayers = 4;
		public Dictionary<string, Player> list = new Dictionary<string, Player>();
		public RoomOptions Options = new RoomOptions();

		public bool AddPlayer(Player player)
		{
			lock(list)
			{
				if (list.Count >= maxPlayers)
					return false;
				PlayerTempData tempData = player.tempData;
				tempData.room = this;
				tempData.team = SwichTeam();
				tempData.status = PlayerTempData.Status.Room;

				if (list.Count == 0)
					tempData.isOwner = true;
				string id = player.id;
				list.Add(id, player);
			}
			return true;
		}

		public int SwichTeam()
		{
			int count1 = 0;
			int count2 = 0;
			foreach (Player player in list.Values)
			{
				if (player.tempData.team == 1) count1++;
				if (player.tempData.team == 2) count2++;
			}

			return (count1 <= count2) ? 1 : 2;
		}

		public void DelPlayer(string id)
		{
			lock(list)
			{
				if (!list.ContainsKey(id))
					return;
				bool isOwner = list[id].tempData.isOwner;
				list[id].tempData.status = PlayerTempData.Status.None;
				list.Remove(id);
				if (isOwner)
					UpdateOwner();
			}
		}

		public void UpdateOwner()
		{
			lock(list)
			{
				if (list.Count <= 0)
					return;
				foreach (Player player in list.Values)
				{
					player.tempData.isOwner = false;
				}

				Player p = list.Values.First();
				p.tempData.isOwner = true;
			}
		}

		public void Broadcast(ProtocolBase protocol)
		{
			foreach (Player player in list.Values)
			{
				player.Send(protocol);
			}
		}

		public ProtocolJson GetRoomInfo()
		{
			JsonData ServerData = new JsonData();
			ServerData["ServerProtoCol"] = "GetRoomInfo";
			ServerData["RoomInfo"] = new JsonData();

			foreach (Player player in list.Values)
			{
				JsonData data = new JsonData();
				data["UserName"] = player.id;
				data["Team"] = player.tempData.team;
				data["Win"] = player.data.win;
				data["Fail"] = player.data.fail;
				data["IsOwner"] = player.tempData.isOwner ? 1 : 0;
				ServerData["RoomInfo"].Add(data);
			}
			ProtocolJson protocol = new ProtocolJson(ServerData.ToJson());
			return protocol;
		}
	}
}
