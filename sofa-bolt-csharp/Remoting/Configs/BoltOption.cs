namespace Remoting.Config
{

	/// <summary>
	/// The base implementation class of the configuration item.
	/// </summary>
	public class BoltOption
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly string name_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private object defaultValue_Renamed;

		protected internal BoltOption(string name, object defaultValue)
		{
			name_Renamed = name;
			defaultValue_Renamed = defaultValue;
		}

		public virtual string name()
		{
			return name_Renamed;
		}

		public virtual object defaultValue()
		{
			return defaultValue_Renamed;
		}

		public static BoltOption valueOf(string name)
		{
			return new BoltOption(name, null);
		}

		public static BoltOption valueOf(string name, object defaultValue)
		{
			return new BoltOption(name, defaultValue);
		}

		public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}

			if (o == null || GetType() != o.GetType())
			{
				return false;
			}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: BoltOption<?> that = (BoltOption<?>) o;
			BoltOption that = (BoltOption) o;

			return !ReferenceEquals(name_Renamed, null) ? name_Renamed.Equals(that.name_Renamed) : ReferenceEquals(that.name_Renamed, null);
		}

		public override int GetHashCode()
		{
			return !ReferenceEquals(name_Renamed, null) ? name_Renamed.GetHashCode() : 0;
		}
	}
}