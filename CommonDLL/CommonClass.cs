using LitJson;
namespace CommonDLL
{
	public class Protocol
	{
		public string ServerProtoCol = string.Empty;
	}

	public class RegisterData : Protocol
	{
		public string UserName = string.Empty;
		public string PassWord = string.Empty;
	}

	public class LoginData : Protocol
	{
		public string UserName = string.Empty;
		public string PassWord = string.Empty;
	}

	public class CreateRoomData : Protocol
	{
		public string RoomName = string.Empty;
	}
}

