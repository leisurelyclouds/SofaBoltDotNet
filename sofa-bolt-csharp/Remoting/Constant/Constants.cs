namespace Remoting.Constant
{
	/// <summary>
	/// Bolt Constants.
	/// </summary>
	public class Constants
	{
		/// <summary>
		/// default expire time to remove connection pool, time unit: milliseconds
		/// </summary>
		public const int DEFAULT_EXPIRE_TIME = 10 * 60 * 1000;

		/// <summary>
		/// default retry times when failed to get result of FutureTask
		/// </summary>
		public const int DEFAULT_RETRY_TIMES = 2;
	}
}