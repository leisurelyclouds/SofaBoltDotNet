using System.Collections;

namespace Remoting.Config.switches
{
	/// <summary>
	/// Global switches used in client or server
	/// <para>
	/// NOTICE:<br>
	/// 1. system settings will take effect in all bolt client or server instances in one process<br>
	/// 2. user settings will only take effect in the current instance of bolt client or server.
	/// </para>
	/// </summary>
	public class GlobalSwitch : Switch
	{

		// switches
		public const int CONN_RECONNECT_SWITCH = 0;
		public const int CONN_MONITOR_SWITCH = 1;
		public const int SERVER_MANAGE_CONNECTION_SWITCH = 2;
		public const int SERVER_SYNC_STOP = 3;

		/// <summary>
		/// user settings
		/// </summary>
		private BitArray userSettings = new BitArray(32);

		/// <summary>
		/// Init with system default value
		///   if settings exist by system property then use system property at first;
		///   if no settings exist by system property then use default value in <seealso cref="config.Configs"/>
		/// All these settings can be overwrite by user api settings.
		/// </summary>
		public GlobalSwitch()
		{
			if (ConfigManager.conn_reconnect_switch())
			{
				userSettings.Set(CONN_RECONNECT_SWITCH, true);
			}
			else
			{
				userSettings.Set(CONN_RECONNECT_SWITCH, false);
			}

			if (ConfigManager.conn_monitor_switch())
			{
				userSettings.Set(CONN_MONITOR_SWITCH, true);
			}
			else
			{
				userSettings.Set(CONN_MONITOR_SWITCH, false);
			}
		}

		// ~~~ public methods
		public virtual void turnOn(int index)
		{
			userSettings.Set(index, true);
		}

		public virtual void turnOff(int index)
		{
			userSettings.Set(index, false);
		}

		public virtual bool isOn(int index)
		{
			return userSettings.Get(index);
		}
	}
}