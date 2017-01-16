using System;
using CommonDLL;
using LitJson;

namespace GameServer_MJ
{
	public partial class HandlePlayerMsg
	{
		public void MsgGetScore(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string ServerName = protocol.GetName();

			JsonData SendData = new JsonData();
			SendData["Score"] = player.data.score;
			player.Send(ServerName, SendData);
			Console.WriteLine(string.Format("MsgGetScore 用户名: {0} 分数: {1}", player.id, player.data.score));
		}

		public void MsgAddScore(Player player, ProtocolBase protoBase)
		{
			//ProtocolJson protocol = protoBase as ProtocolJson;

			player.data.score += 1;
			Console.WriteLine(string.Format("MsgAddScore 用户名: {0} 分数: {1}", player.id, player.data.score));
		}

		public void MsgGetAchieve(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string ServerName = protocol.GetName();

			JsonData SendData = new JsonData();
			SendData["Win"] = player.data.win;
			SendData["Fail"] = player.data.fail;
			player.Send(ServerName, SendData);
			Console.WriteLine(string.Format("MsgGetScore id:{0} win:{1} fail:{2}", player.id, player.data.win, player.data.fail));
		}
	}
}
