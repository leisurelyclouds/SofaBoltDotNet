namespace Remoting
{

	/// <summary>
	/// Remoting address parser
	/// 
	/// Implement this to generate a <seealso cref="Url"/>
	/// </summary>
	public interface RemotingAddressParser
	{
		/// <summary>
		/// Parse a simple string url to get <seealso cref="Url"/>
		/// </summary>
		/// <param name="url"> </param>
		/// <returns> parsed <seealso cref="Url"/> </returns>
		Url parse(string url);

		/// <summary>
		/// Parse a simple string url to get a unique key of a certain address
		/// </summary>
		/// <param name="url">
		/// @return </param>
		string parseUniqueKey(string url);

		/// <summary>
		/// Parse to get property value according to specified property key
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="propKey"> </param>
		/// <returns> propValue </returns>
		string parseProperty(string url, string propKey);

		/// <summary>
		/// Initialize <seealso cref="Url"/> arguments
		/// </summary>
		/// <param name="url"> </param>
		void initUrlArgs(Url url);

		/// <summary>
		/// symbol :
		/// </summary>

		/// <summary>
		/// symbol =
		/// </summary>

		/// <summary>
		/// symbol &
		/// </summary>

		/// <summary>
		/// symbol ?
		/// </summary>
	}

	public static class RemotingAddressParser_Fields
	{
		public const char COLON = ':';
		public const char EQUAL = '=';
		public const char AND = '&';
		public const char QUES = '?';
	}
}