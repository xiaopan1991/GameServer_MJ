﻿using System;
using System.Net.Sockets;
using CommonDLL;
using LitJson;

namespace GameServer_MJ
{
	public class Conn
	{
		public const int BUFFER_SIZE = 1024;

		public byte[] readBuff = new byte[BUFFER_SIZE];
		public int buffCount = 0;

		public byte[] lenBytes = new byte[sizeof(UInt32)];
		public Int32 msgLength = 0;
		public long lastTickTime = long.MinValue;

		public Socket socket;
		public Player player;
		public bool isUse = false;

		public Conn()
		{
			readBuff = new byte[BUFFER_SIZE];
		}

		public void Init(Socket socket)
		{
			this.socket = socket;
			isUse = true;
			buffCount = 0;

			lastTickTime = Sys.GetTimeStamp();
		}
		private void ResetValue()
		{
			this.socket = null;
			isUse = false;
			buffCount = 0;
			lastTickTime = 0;
		}

		public int BuffRemain()
		{
			return BUFFER_SIZE - buffCount;
		}

		public string GetAdress()
		{
			if (!isUse)
				return "无法获取地址";
			return socket.RemoteEndPoint.ToString();
		}
		public void Close()
		{
			if (!isUse)
				return;
			if (player != null)
			{
				//player.Logout();
			}
			Console.WriteLine("[断开连接]" + GetAdress());

			//socket.Shutdown(SocketShutdown.Both);
			socket.Close();

			ResetValue();
		}

		public void Send(string ServerName, JsonData data = null)
		{
			if (data == null)
				data = new JsonData();
			data["ServerProtoCol"] = ServerName;

			ProtocolJson protocol = new ProtocolJson(data.ToJson());
			ServerNet.GetInstance().Send(this, protocol);
		}
	}
}
