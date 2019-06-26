using System;


namespace com.alipay.remoting.rpc.common
{


    /// <summary>
    /// biz request as a demo
    /// 


    /// </summary>
    [Serializable]
	public class RequestBody
	{

		/// <summary>
		/// for serialization
		/// </summary>
		private const long serialVersionUID = -1288207208017808618L;

		public const string DEFAULT_CLIENT_STR = "HELLO WORLD! I'm from client";
		public const string DEFAULT_SERVER_STR = "HELLO WORLD! I'm from server";
		public const string DEFAULT_SERVER_RETURN_STR = "HELLO WORLD! I'm server return";
		public const string DEFAULT_CLIENT_RETURN_STR = "HELLO WORLD! I'm client return";

		public const string DEFAULT_ONEWAY_STR = "HELLO WORLD! I'm oneway req";
		public const string DEFAULT_SYNC_STR = "HELLO WORLD! I'm sync req";
		public const string DEFAULT_FUTURE_STR = "HELLO WORLD! I'm future req";
		public const string DEFAULT_CALLBACK_STR = "HELLO WORLD! I'm call back req";

		/// <summary>
		/// id
		/// </summary>
		private int id;

		/// <summary>
		/// msg
		/// </summary>
		private string msg;

		/// <summary>
		/// body
		/// </summary>
		private byte[] body;

		private Random r = new Random();

		public RequestBody()
		{
			//json serializer need default constructor
		}

		public RequestBody(int id, string msg)
		{
			this.id = id;
			this.msg = msg;
		}

		public RequestBody(int id, int size)
		{
			this.id = id;
			this.msg = "";
			this.body = new byte[size];
			r.NextBytes(this.body);
		}

		/// <summary>
		/// Getter method for property <tt>id</tt>.
		/// </summary>
		/// <returns> property value of id </returns>
		public virtual int Id
		{
			get
			{
				return id;
			}
			set
			{
				this.id = value;
			}
		}


		/// <summary>
		/// Getter method for property <tt>msg</tt>.
		/// </summary>
		/// <returns> property value of msg </returns>
		public virtual string Msg
		{
			get
			{
				return msg;
			}
			set
			{
				this.msg = value;
			}
		}


		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString()
		{
			return "Body[this.id = " + id + ", this.msg = " + msg + "]";
		}

		public enum InvokeType
		{
			ONEWAY,
			SYNC,
			FUTURE,
			CALLBACK
		}
	}
}