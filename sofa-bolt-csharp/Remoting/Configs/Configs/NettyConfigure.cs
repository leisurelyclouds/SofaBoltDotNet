namespace Remoting.Config.configs
{

	/// <summary>
	/// netty related configuration items
	/// </summary>
	public interface NettyConfigure
	{
		/// <summary>
		/// Initialize netty write buffer water mark for remoting instance.
		/// <para>
		/// Notice: This api should be called before init remoting instance.
		/// 
		/// </para>
		/// </summary>
		/// <param name="low"> [0, high] </param>
		/// <param name="high"> [high, Integer.MAX_VALUE) </param>
		void initWriteBufferWaterMark(int low, int high);

		/// <summary>
		/// get the low water mark for netty write buffer
		/// </summary>
		/// <returns> low watermark </returns>
		int netty_buffer_low_watermark();

		/// <summary>
		/// get the high water mark for netty write buffer
		/// </summary>
		/// <returns> high watermark </returns>
		int netty_buffer_high_watermark();
	}
}