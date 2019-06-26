using System;


namespace com.alipay.remoting.rpc.userprocessor.multiinterestprocessor
{

    /// <summary>
    /// @antuor muyun.cyt (muyun.cyt@antfin.com)  2018/7/5   11:20 AM
    /// </summary>
    [Serializable]
	public class RequestBodyC2 : MultiInterestBaseRequestBody
	{

		private const long serialVersionUID = -7512540836127308096L;

		public const string DEFAULT_CLIENT_STR = "HELLO WORLD! I'm from client--C2";
		public const string DEFAULT_SERVER_STR = "HELLO WORLD! I'm from server--C2";
		public const string DEFAULT_SERVER_RETURN_STR = "HELLO WORLD! I'm server return--C2";
		public const string DEFAULT_CLIENT_RETURN_STR = "HELLO WORLD! I'm client return--C2";

		public const string DEFAULT_ONEWAY_STR = "HELLO WORLD! I'm oneway req--C2";
		public const string DEFAULT_SYNC_STR = "HELLO WORLD! I'm sync req--C2";
		public const string DEFAULT_FUTURE_STR = "HELLO WORLD! I'm future req--C2";
		public const string DEFAULT_CALLBACK_STR = "HELLO WORLD! I'm call back req--C2";

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

		public RequestBodyC2()
		{
			//json serializer need default constructor
			//super();
		}

		public RequestBodyC2(int id, string msg)
		{
			//super(id, msg);
			this.id = id;
			this.msg = msg;
		}

		public RequestBodyC2(int id, int size)
		{
			// super(id, size);
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
				return this.id;
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
				return this.msg;
			}
			set
			{
				this.msg = value;
			}
		}


		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString()
		{
			return "Body[this.id = " + this.id + ", this.msg = " + this.msg + "]";
		}
	}

}