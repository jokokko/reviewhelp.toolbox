namespace ReviewHelp.Toolbox.Model
{
	public sealed class StructureMapLifecycle
	{
		public string Name { get; }

		public static readonly StructureMapLifecycle Singleton = new StructureMapLifecycle("Singleton");
		public static readonly StructureMapLifecycle ChildContainerSingleton = new StructureMapLifecycle("ChildContainerSingleton");
		public static readonly StructureMapLifecycle Transient = new StructureMapLifecycle("Transient");
		public static readonly StructureMapLifecycle TransientImplicit = new StructureMapLifecycle("Transient (implicit, SM default)");
		public static readonly StructureMapLifecycle Unique = new StructureMapLifecycle("Unique");
		public static readonly StructureMapLifecycle Container = new StructureMapLifecycle("Container");
		public static readonly StructureMapLifecycle ThreadLocal = new StructureMapLifecycle("ThreadLocal");
		private StructureMapLifecycle(string name)
		{
			Name = name;
		}

		public static implicit operator string(StructureMapLifecycle value)
		{
			return value.Name;
		}
		private bool Equals(StructureMapLifecycle other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is StructureMapLifecycle tag && Equals(tag);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}