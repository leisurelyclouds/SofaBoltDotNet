using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace Remoting.util
{
	/// <summary>
	/// connection util
	/// </summary>
	public class ConnectionUtil
	{
		public static Connection getConnectionFromChannel(IChannel channel)
		{
			if (channel == null)
			{
				return null;
			}

			IAttribute<Connection> connAttr = channel.GetAttribute(Connection.CONNECTION);
			if (connAttr != null)
			{
				Connection connection = connAttr.Get();
				return connection;
			}
			return null;
		}

		public static void addIdPoolKeyMapping(int? id, string group, IChannel channel)
		{
			Connection connection = getConnectionFromChannel(channel);
			if (connection != null)
			{
				connection.addIdPoolKeyMapping(id, group);
			}
		}

		public static string removeIdPoolKeyMapping(int? id, IChannel channel)
		{
			Connection connection = getConnectionFromChannel(channel);
			if (connection != null)
			{
				return connection.removeIdPoolKeyMapping(id);
			}

			return null;
		}

		public static void addIdGroupCallbackMapping(int? id, InvokeFuture callback, IChannel channel)
		{
			Connection connection = getConnectionFromChannel(channel);
			if (connection != null)
			{
				connection.addInvokeFuture(callback);
			}
		}

		public static InvokeFuture removeIdGroupCallbackMapping(int? id, IChannel channel)
		{
			Connection connection = getConnectionFromChannel(channel);
			if (connection != null)
			{
				return connection.removeInvokeFuture(id.Value);
			}
			return null;
		}

		public static InvokeFuture getGroupRequestCallBack(int? id, IChannel channel)
		{
			Connection connection = getConnectionFromChannel(channel);
			if (connection != null)
			{
				return connection.getInvokeFuture(id.Value);
			}

			return null;
		}
	}
}