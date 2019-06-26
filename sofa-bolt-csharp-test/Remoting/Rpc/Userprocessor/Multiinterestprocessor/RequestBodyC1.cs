using System;


namespace com.alipay.remoting.rpc.userprocessor.multiinterestprocessor
{

    /// <summary>
    /// @antuor muyun.cyt (muyun.cyt@antfin.com)  2018/7/5   11:20 AM
    /// </summary>
    [Serializable]
	public class RequestBodyC1 : MultiInterestBaseRequestBody
	{

		private const long serialVersionUID = -103461930947826245L;

		public const string DEFAULT_CLIENT_STR = "HELLO WORLD! I'm from client--C1";
		public const string DEFAULT_SERVER_STR = "HELLO WORLD! I'm from server--C1";
		public const string DEFAULT_SERVER_RETURN_STR = "HELLO WORLD! I'm server return--C1";
		public const string DEFAULT_CLIENT_RETURN_STR = "HELLO WORLD! I'm client return--C1";

		public const string DEFAULT_ONEWAY_STR = "HELLO WORLD! I'm oneway req--C1";
		public const string DEFAULT_SYNC_STR = "HELLO WORLD! I'm sync req--C1";
		public const string DEFAULT_FUTURE_STR = "HELLO WORLD! I'm future req--C1";
		public const string DEFAULT_CALLBACK_STR = "HELLO WORLD! I'm call back req--C1";

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

		public RequestBodyC1()
		{
			//json serializer need default constructor
		}

		public RequestBodyC1(int id, string msg)
		{
			this.id = id;
			this.msg = msg;
		}

		public RequestBodyC1(int id, int size)
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