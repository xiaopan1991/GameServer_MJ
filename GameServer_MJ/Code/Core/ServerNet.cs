using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Timers;
using System.Reflection;
using CommonDLL;

namespace GameServer_MJ
{
	public class ServerNet
	{
		public Socket listenfd;
		public Conn[] conns;
		public int maxConn = 50;

		public HandleConnMsg handleConnMsg = new HandleConnMsg();
		public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
		public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();

		private static ServerNet instance = null;
		private Timer timer;
		private long heartBeatTime = 100;
		private ProtocolBase proto;


		public static ServerNet GetInstance()
		{
			if (instance == null)
				instance = new ServerNet();
			return instance;
		}

		public ServerNet()
		{
			timer = new Timer(1000);
		}

		//通过协议类型 初始化 协议数据
		public void Init(ProtocolBaseType type)
		{
			switch (type)
			{
				case ProtocolBaseType.String:
					proto = new ProtocolStr();
					break;
				case ProtocolBaseType.Json:
					proto = new ProtocolJson();
					break;
				default:
					break;
			}
		}

		public void Start(string host, int port)
		{
			//timer.Elapsed += new ElapsedEventHandler(HandleMainTimer);
			//timer.AutoReset = false;
			//timer.Enabled = true;

			conns = new Conn[maxConn];
			for (int i = 0; i < maxConn; i++)
			{
				conns[i] = new Conn();
			}
			listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress ipAdr = IPAddress.Parse(host);
			IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
			listenfd.Bind(ipEp);
			listenfd.Listen(maxConn);

			listenfd.BeginAccept(AcceptCallback, null);
			Console.WriteLine("[服务器]启动成功");
		}

		public void InitAndStart(string host, int port, ProtocolBaseType type)
		{
			Init(type);
			Start(host, port);
		}

		public int NewIndex()
		{
			if (conns == null)
				return -1;
			for (int i = 0; i < conns.Length; i++)
			{
				if (conns[i] == null)
				{
					conns[i] = new Conn();
					return i;
				}
				else if (!conns[i].isUse)
				{
					return i;
				}
			}
			return -1;
		}

		private void HandleMainTimer(object sender, ElapsedEventArgs args)
		{
			//心跳
			HeartBeat();
			timer.Start();
		}

		private void HeartBeat()
		{
			//Console.WriteLine("[主定时器执行]");
			long timeNow = Sys.GetTimeStamp();

			for (int i = 0; i < conns.Length; i++)
			{
				Conn conn = conns[i];
				if (conn == null) continue;
				if (!conn.isUse) continue;

				//if (conn.lastTickTime < timeNow - heartBeatTime)
				//{
				//	Console.WriteLine("[心跳引起断开网络连接]" + conn.GetAdress());
				//	lock (conn)
				//		conn.Close();
				//}
			}
		}

		private void AcceptCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = listenfd.EndAccept(ar);
				int index = NewIndex();

				if (index < 0)
				{
					socket.Close();
					Console.WriteLine("[警告]连接已满");
				}
				else
				{
					Conn conn = conns[index];
					conn.Init(socket);
					string adr = conn.GetAdress();
					Console.WriteLine("客户端连接[" + adr + "] conn池 ID: " + index);
					conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCallback, conn);
				}
				listenfd.BeginAccept(AcceptCallback, null);
			}
			catch (Exception e)
			{
				Console.WriteLine("AcceptCallback 失败 " + e.Message);
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			Conn conn = (Conn)ar.AsyncState;
			try
			{
				int count = conn.socket.EndReceive(ar);
				if (count <= 0)
				{
					Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开连接");
					conn.Close();
					return;
				}

				conn.buffCount += count;
				ProcessData(conn);

				conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCallback, conn);
			}
			catch (Exception e)
			{
				Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开连接 " + e.Message);
				conn.Close();
			}
		}

		public void Close()
		{
			for (int i = 0; i < conns.Length; i++)
			{
				Conn conn = conns[i];
				if (conn == null) continue;
				if (!conn.isUse) continue;
				lock (conn)
				{
					conn.Close();
				}
			}
		}

		private void ProcessData(Conn conn)
		{
			if (conn.buffCount < sizeof(Int32))
				return;

			Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
			conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);

			if (conn.buffCount < conn.msgLength + sizeof(Int32))
				return;

			ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
			HandleMsg(conn, protocol);

			int count = conn.buffCount - conn.msgLength - sizeof(Int32);
			Array.Copy(conn.readBuff, sizeof(Int32), conn.readBuff, 0, count);
			conn.buffCount = count;
			if (conn.buffCount > 0)
				ProcessData(conn);
		}

		private void HandleMsg(Conn conn, ProtocolBase protoBase)
		{
			string name = protoBase.GetName();
			Console.WriteLine("[收到协议]" + name);
			string methodName = "Msg" + name;

			if (conn.player == null || name == "HeartBeat" || name == "Logout")
			{
				MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
				if (mm == null)
				{
					string str = "[警告]HandleMsg 没有处理连接的方法";
					Console.WriteLine(str + methodName);
					return;
				}
				object[] obj = new object[] { conn, protoBase };
				Console.WriteLine("[处理连接消息] " + conn.GetAdress() + ":" + name);
				mm.Invoke(handleConnMsg, obj);
			}
			else
			{
				MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
				if (mm == null)
				{
					string str = "[警告]HandleMsg 没有处理连接的方法";
					Console.WriteLine(str + methodName);
					return;
				}
				object[] obj = new object[] { conn.player, protoBase };
				Console.WriteLine("[处理连接消息] " + conn.player.id + ":" + name);
				mm.Invoke(handlePlayerMsg, obj);
			}

			//if (name == "HeartBeat")
			//{
			//	Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
			//	conn.lastTickTime = Sys.GetTimeStamp();
			//}
			//Send(conn, protoBase);
		}

		public void Send(Conn conn, ProtocolBase protoBase)
		{
			byte[] bytes = protoBase.Encode();
			byte[] length = BitConverter.GetBytes(bytes.Length);
			byte[] sendbuff = length.Concat(bytes).ToArray();
			try
			{
				conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
			}
			catch (Exception e)
			{
				Console.WriteLine("[发送消息]" + conn.GetAdress() + ":" + e.Message);
			}
		}
		private void Broadcast(ProtocolBase protocol)
		{
			for (int i = 0; i < conns.Length; i++)
			{
				if (!conns[i].isUse) continue;
				if (conns[i].player == null) continue;
				Send(conns[i], protocol);
			}
		}

		public void Print()
		{
			Console.WriteLine("===服务器登录信息===");
			for (int i = 0; i < conns.Length; i++)
			{
				if (conns[i] == null) continue;
				if (!conns[i].isUse) continue;
				string str = string.Format("连接[{0}]", conns[i].GetAdress());

				if (conns[i].player != null)
					str += string.Format("玩家 id: {0}", conns[i].player.id);

				Console.WriteLine(str);
			}
		}
	}
}
