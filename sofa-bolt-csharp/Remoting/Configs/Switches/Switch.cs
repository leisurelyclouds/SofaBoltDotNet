namespace Remoting.Config.switches
{
	/// <summary>
	/// switch interface
	/// </summary>
	public interface Switch
	{
		/// <summary>
		/// api for user to turn on a feature
		/// </summary>
		/// <param name="index"> the switch index of feature </param>
		void turnOn(int index);

		/// <summary>
		/// api for user to turn off a feature
		/// </summary>
		/// <param name="index"> the switch index of feature </param>
		void turnOff(int index);

		/// <summary>
		/// check switch whether on
		/// </summary>
		/// <param name="index"> the switch index of feature </param>
		/// <returns> true if either system setting is on or user setting is on </returns>
		bool isOn(int index);
	}
}