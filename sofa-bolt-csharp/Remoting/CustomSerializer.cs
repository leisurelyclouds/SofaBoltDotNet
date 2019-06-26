using Remoting.rpc;

namespace Remoting
{
	/// <summary>
	/// Define custom serializers for command header and content.
	/// </summary>
	public interface CustomSerializer
	{
		/// <summary>
		/// Serialize the header of RequestCommand.
		/// </summary>
		/// <param name="request"> </param>
		/// <param name="invokeContext">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool serializeHeader(RequestCommand request, InvokeContext invokeContext);

		/// <summary>
		/// Serialize the header of ResponseCommand.
		/// </summary>
		/// <param name="response">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool serializeHeader(ResponseCommand response);

		/// <summary>
		/// Deserialize the header of RequestCommand.
		/// </summary>
		/// <param name="request">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool deserializeHeader(RequestCommand request);

		/// <summary>
		/// Deserialize the header of ResponseCommand.
		/// </summary>
		/// <param name="response"> </param>
		/// <param name="invokeContext">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool deserializeHeader(ResponseCommand response, InvokeContext invokeContext);

		/// <summary>
		/// Serialize the content of RequestCommand.
		/// </summary>
		/// <param name="request"> </param>
		/// <param name="invokeContext">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool serializeContent(RequestCommand request, InvokeContext invokeContext);

		/// <summary>
		/// Serialize the content of ResponseCommand.
		/// </summary>
		/// <param name="response">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool serializeContent(ResponseCommand response);

		/// <summary>
		/// Deserialize the content of RequestCommand.
		/// </summary>
		/// <param name="request">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool deserializeContent(RequestCommand request);

		/// <summary>
		/// Deserialize the content of ResponseCommand.
		/// </summary>
		/// <param name="response"> </param>
		/// <param name="invokeContext">
		/// @return </param>
		/// <exception cref="CodecException"> </exception>
		bool deserializeContent(ResponseCommand response, InvokeContext invokeContext);
	}
}