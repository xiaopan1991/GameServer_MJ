using System;
using LitJson;
using CommonDLL;

namespace GameServer_MJ
{
	public partial class HandlePlayerMsg
	{
		public void MsgGetRoomList(Player player, ProtocolBase protoBase)
		{
			player.Send(RoomManager.GetInstance().GetRoomList());
		}

		public void MsgCreateRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			CreateRoomData createRoomData = protocol.GetData<CreateRoomData>();

			ProtocolJson protocolRet;
			JsonData rData = new JsonData();
			rData["ServerProtoCol"] = createRoomData.ServerProtoCol;

			if (player.tempData.status != PlayerTempData.Status.None)
			{
				rData["State"] = -1;
				protocolRet = new ProtocolJson(rData.ToJson());
				player.Send(protocolRet);

				Console.WriteLine(string.Format("MsgCreateRoom Fail; id:{0}", player.id));
				return;
			}

			RoomOptions options = new RoomOptions(createRoomData.RoomName);
			RoomManager.GetInstance().CreateRoom(player, options);

			rData["State"] = 0;
			rData["RoomList"] = new JsonData();
			var list = RoomManager.GetInstance().list;
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Room room = list[i];
				JsonData jdata = new JsonData();
				jdata["RoomName"] = room.Options.RoomName;
				jdata["Count"] = room.list.Count;
				jdata["Status"] = (int)room.status;
				rData["RoomList"].Add(jdata);
			}
			protocolRet = new ProtocolJson(rData.ToJson());
			player.Send(protocolRet);
			Console.WriteLine(string.Format("MagCreateRoom Seccuss; id:{0}", player.id));
		}

		public void MsgEnterRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string protoName = protocol.GetName();

			JsonData ClientData = protocol.GetJsonData();
			int index = (int)ClientData["Index"];
			Console.WriteLine("[收到MsgEnterRoom] " + player.id + " " + index);

			JsonData ServerData = new JsonData();
			ServerData["ServerProtoCol"] = protoName;

			if (index < 0 || index >= RoomManager.GetInstance().list.Count)
			{
				Console.WriteLine(string.Format("MsgEnterRoom index err; id:{0}", player.id));
				ServerData["State"] = -1;

				protocol = new ProtocolJson(ServerData.ToJson());
				player.Send(protocol);
				return;
			}

			Room room = RoomManager.GetInstance().list[index];
			if (room.status != Room.Status.Prepare)
			{
				Console.WriteLine(string.Format("MsgEnterRoom status err; id:{0}", player.id));
				ServerData["State"] = -1;

				protocol = new ProtocolJson(ServerData.ToJson());
				player.Send(protocol);
				return;
			}

			if (room.AddPlayer(player))
			{
				room.Broadcast(RoomManager.GetInstance().GetRoomList());

				ServerData["State"] = 0;
				protocol = new ProtocolJson(ServerData.ToJson());
				player.Send(protocol);
			}
			else
			{
				Console.WriteLine("MsgEnterRoom maxPlayer err; id:{0}", player.id);
				ServerData["State"] = -1;
				protocol = new ProtocolJson(ServerData.ToJson());
				player.Send(protocol);
			}
		}

		public void MsgGetRoomInfo(Player player, ProtocolBase protoBase)
		{
			if (player.tempData.status != PlayerTempData.Status.Room)
			{
				Console.WriteLine(string.Format("MsgGetRoomInfo status err; id:{0}", player.id));
				return;
			}
			Room room = player.tempData.room;
			player.Send(room.GetRoomInfo());
		}

		public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string protocolName = protocol.GetName();

			JsonData Jdata = new JsonData();
			Jdata["ServerProtoCol"] = protocolName;

			ProtocolJson protocolRet;

			if (player.tempData.status != PlayerTempData.Status.Room)
			{
				Console.WriteLine(string.Format("MsgLeaveRoom status err; id:{0}", player.id));
				Jdata["State"] = -1;
				protocolRet = new ProtocolJson(Jdata.ToJson());
				player.Send(protocolRet);
				return;
			}

			Jdata["State"] = 0;
			protocolRet = new ProtocolJson(Jdata.ToJson());
			player.Send(protocolRet);
			Room room = player.tempData.room;
			RoomManager.GetInstance().LeaveRoom(player);

			if (room != null)
				room.Broadcast(room.GetRoomInfo());
		}
	}
}
