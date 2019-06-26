using java.util.concurrent.atomic;

namespace Remoting.util
{
	/// <summary>
	/// IDGenerator is used for generating request id in integer form.
	/// </summary>
	public class IDGenerator
	{
		private static readonly AtomicInteger id = new AtomicInteger(0);

		/// <summary>
		/// generate the next id
		/// </summary>
		public static int nextId()
		{
			return id.incrementAndGet();
		}

		public static void resetId()
		{
			id.set(0);
		}
	}
}