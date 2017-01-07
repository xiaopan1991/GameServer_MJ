using System;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SocketTest
{
	public class DataManager
	{
		private MySqlConnection sqlConn;

		private static DataManager instance = null;

		public static DataManager GetInstance()
		{
			if (instance == null)
				instance = new DataManager();
			return instance;
		}

		public void Init()
		{
			Connect();
		}

		private void Connect()
		{
			string connStr = "Database=msgboard;Data Source=127.0.0.1;";
			connStr += "User Id=root;Password=911112;port=3306";
			sqlConn = new MySqlConnection(connStr);
			try
			{
				sqlConn.Open();
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] Connect " + e.Message);
				return;
			}
		}

		public bool IsSafeStr(string str)
		{
			return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
		}

		//检查是否存在该用户
		private bool CanRegister(string id)
		{
			if (!IsSafeStr(id))
				return false;

			string cmdStr = string.Format("select * from user where id='{0}'", id);
			MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
			try 
			{
				MySqlDataReader dataReader = cmd.ExecuteReader();
				bool hasRows = dataReader.HasRows;
				dataReader.Close();
				return !hasRows;
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] CanRegister fail " + e.Message);
				return false;
			}
		}

		//注册
		public bool Register(string id, string pw)
		{
			if (!IsSafeStr(id) || !IsSafeStr(pw))
			{
				Console.WriteLine("[DataManager] Register 使用非法字符");
				return false;
			}

			if (!CanRegister(id))
			{
				Console.WriteLine("[DataManager] Register !CanRegister");
				return false;
			}

			string cmdStr = string.Format("insert into user set id ='{0}', pw='{1}';", id, pw);
			MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

			try {
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] Register " + e.Message);
				return false;
			}
		}

		//创建角色
		public bool CreatePlayer(string id)
		{
			if (!IsSafeStr(id))
				return false;

			IFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			PlayerData playerData = new PlayerData();

			try
			{
				formatter.Serialize(stream, playerData);
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] CreatePlayer 序列化 " + e.Message);
				return false;
			}

			byte[] byteArr = stream.ToArray();

			//写入数据库
			string cmdStr = string.Format("insert into player set id = '{0}', data=@data;", id);
			MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
			cmd.Parameters.Add("@data", MySqlDbType.Blob);
			cmd.Parameters[0].Value = byteArr;
			try
			{
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] CreatePlayer 写入 " + e.Message);
				return false;
			}
		}

		//检测用户名和密码
		public bool CheckPassWord(string id, string pw)
		{
			if (!IsSafeStr(id) || !IsSafeStr(pw))
				return false;

			string cmdStr = string.Format("select * from user where id='{0}' and pw='{1}';", id, pw);
			MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
			try
			{
				MySqlDataReader dataReader = cmd.ExecuteReader();
				bool hasRows = dataReader.HasRows;
				dataReader.Close();
				return hasRows;
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] CreatePlayer 写入 " + e.Message);
				return false;
			}
		}

		//获取玩家数据
		public PlayerData GetPlayerData(string id)
		{
			PlayerData playerData = null;

			if (!IsSafeStr(id))
				return playerData;

			string cmdStr = string.Format("select * from player where id ='{0}';", id);
			MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
			byte[] buffer = new byte[1];
			try
			{
				MySqlDataReader dataReader = cmd.ExecuteReader();
				if (!dataReader.HasRows)
				{
					dataReader.Close();
					return playerData;
				}
				dataReader.Read();

				long len = dataReader.GetBytes(1, 0, null, 0, 0);
				buffer = new byte[len];
				dataReader.GetBytes(1, 0, buffer, 0, (int)len);
				dataReader.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine("[DataManager] GetPlayerData 查询 " + ex.Message);
				return playerData;
			}

			MemoryStream stream = new MemoryStream(buffer);
			try
			{
				BinaryFormatter formatter = new BinaryFormatter();
				playerData = (PlayerData)formatter.Deserialize(stream);
				return playerData;
			}
			catch (Exception ex)
			{
				Console.WriteLine("[DataManager] GetPlayerData 反序列化 " + ex.Message);
				return playerData;
			}
		}

		//保存角色
		public bool SavePlayer(Player player)
		{
			PlayerData playerData = player.data;

			IFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			try
			{
				formatter.Serialize(stream, playerData);
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] SavePlayer 序列化 " + e.Message);
				return false;
			}

			byte[] byteArr = stream.ToArray();

			string formatStr = "update player set data =@data where id = '{0}';";
			string cmdStr = string.Format(formatStr, player.id);
			MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
			cmd.Parameters.Add("@data", MySqlDbType.Blob);
			cmd.Parameters[0].Value = byteArr;
			try
			{
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("[DataManager] SavePlayer 写入 " + e.Message);
				return false;
			}
		}
	}
}
