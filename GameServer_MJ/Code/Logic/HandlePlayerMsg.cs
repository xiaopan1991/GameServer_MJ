using System;
namespace SocketTest
{
	public partial class HandlePlayerMsg
	{
		public void MsgGetScore(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocolRet = new ProtocolBytes();
			protocolRet.AddString("GetScore");
			protocolRet.AddInt(player.data.score);
			player.Send(protocolRet);
			Console.WriteLine(string.Format("MsgGetScore 用户名: {0} 分数: {1}", player.id, player.data.score));
		}

		public void MsgAddScore(Player player, ProtocolBase protoBase)
		{
			int start = 0;
			ProtocolBytes protocol = protoBase as ProtocolBytes;
			//string protoName = protocol.GetString(start, ref start);
			protocol.GetString(start, ref start);

			player.data.score += 1;
			Console.WriteLine(string.Format("MsgAddScore 用户名: {0} 分数: {1}", player.id, player.data.score));
		}

		public void MsgGetAchieve(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocolRet = new ProtocolBytes();
			protocolRet.AddString("GetAchieve");
			protocolRet.AddInt(player.data.win);
			protocolRet.AddInt(player.data.fail);
			player.Send(protocolRet);
			Console.WriteLine(string.Format("MsgGetScore id:{0} win:{1} fail:{2}", player.id, player.data.win, player.data.fail));
		}
	}
}
