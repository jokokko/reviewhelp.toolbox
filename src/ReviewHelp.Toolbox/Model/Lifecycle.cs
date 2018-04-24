namespace ReviewHelp.Toolbox.Model
{
	public sealed class Lifecycle
	{
		public string Name { get; }

		public static readonly Lifecycle Singleton = new Lifecycle("Singleton");
		public static readonly Lifecycle ChildContainerSingleton = new Lifecycle("ChildContainerSingleton");
		public static readonly Lifecycle Transient = new Lifecycle("Transient");
		public static readonly Lifecycle TransientImplicit = new Lifecycle("Transient (implicit, SM default)");
		public static readonly Lifecycle Unique = new Lifecycle("Unique");
		public static readonly Lifecycle Container = new Lifecycle("Container");
		public static readonly Lifecycle ThreadLocal = new Lifecycle("ThreadLocal");
		private Lifecycle(string name)
		{
			Name = name;
		}

		public static implicit operator string(Lifecycle value)
		{
			return value.Name;
		}
		private bool Equals(Lifecycle other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Lifecycle tag && Equals(tag);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}