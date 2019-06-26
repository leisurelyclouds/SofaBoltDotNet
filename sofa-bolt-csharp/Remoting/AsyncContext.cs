namespace Remoting
{
	/// <summary>
	/// Async context for biz.
	/// </summary>
	public interface AsyncContext
	{
		/// <summary>
		/// send response back
		/// </summary>
		/// <param name="responseObject"> response object </param>
		void sendResponse(object responseObject);
	}
}