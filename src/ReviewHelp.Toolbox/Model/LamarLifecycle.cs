namespace ReviewHelp.Toolbox.Model
{
	public sealed class LamarLifecycle
	{
		public string Name { get; }

		public static readonly LamarLifecycle Singleton = new LamarLifecycle("Singleton");
		public static readonly LamarLifecycle Scoped = new LamarLifecycle("Scoped");
		public static readonly LamarLifecycle Transient = new LamarLifecycle("Transient");
		public static readonly LamarLifecycle TransientImplicit = new LamarLifecycle("Transient (implicit, Lamar default)");
		private LamarLifecycle(string name)
		{
			Name = name;
		}

		public static implicit operator string(LamarLifecycle value)
		{
			return value.Name;
		}
		private bool Equals(LamarLifecycle other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is LamarLifecycle tag && Equals(tag);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}