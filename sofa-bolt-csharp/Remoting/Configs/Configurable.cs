namespace Remoting.Config
{
	/// <summary>
	/// Config interface.
	/// </summary>
	public interface Configurable
	{
		/// <summary>
		/// Get the option value.
		/// </summary>
		/// <param name="option"> target option </param>
		/// <returns> BoltOption </returns>
		object option(BoltOption option);

		/// <summary>
		/// Allow to specify a <seealso cref="BoltOption"/> which is used for the <seealso cref="Configurable"/> instances once they got
		/// created. Use a value of {@code null} to remove a previous set <seealso cref="BoltOption"/>.
		/// </summary>
		/// <param name="option"> target option </param>
		/// <param name="value"> option value, null to remove the previous option </param>
		/// <returns> Configurable instance </returns>
		Configurable option(BoltOption option, object value);
	}
}