using System.Linq;
using ReviewHelp.Toolbox.Analyzers;
using ReviewHelp.Toolbox.Model;
using ReviewHelp.Toolbox.Tests.Infrastructure;
using Xunit;

namespace ReviewHelp.Toolbox.Tests.Analyzers
{
	public sealed class StructureMapWiringAnalyzerTests
	{
		[Fact]
		public async void CanDetermineWirings()
		{
			var analyzer = new StructureMapWiringAnalyzer();
			await TestHelper.GetDiagnosticsAsync(analyzer,
				@"using System;
using System.Threading;
using System.Threading.Tasks;
using StructureMap;
using StructureMap.Pipeline;

class TestClass { 
	
	void TestMethod() 
	{
		new Container(c =>
			{
				If plugin = null;
				var k = new K();

				c.For<If>().Singleton().Use<T>();
				c.For<If>().LifecycleIs(new SingletonLifecycle()).Use<T>();				
				c.For<If>().LifecycleIs<SingletonLifecycle>().Use<T>();
				c.For(typeof(If)).LifecycleIs(new SingletonLifecycle()).Use(typeof(T));
				c.ForSingletonOf(typeof(If)).Use(typeof(T));
				c.ForSingletonOf<If>().Use<T>();								
				c.For<If>().LifecycleIs<SingletonLifecycle>().Use(new T());
				c.For<If>().LifecycleIs<SingletonLifecycle>().Use(() => new T());
				c.For<If>(new SingletonLifecycle()).Use<T>();
				c.For(typeof(If), new SingletonLifecycle()).Use(typeof(T));
				c.For(plugin.GetType()).LifecycleIs(new SingletonLifecycle()).Use(typeof(T));
				c.ForSingletonOf(typeof(If)).Use(ctx => new K());
				c.For<If>().LifecycleIs<SingletonLifecycle>().Use(() => new T());
				c.ForSingletonOf(typeof(If)).Use(""desc"", ctx => k);				
				c.ForSingletonOf(typeof(If)).Use(typeof(T));

				c.For<If>().AlwaysUnique().Use<T>();
				c.For<If>().LifecycleIs(new UniquePerRequestLifecycle()).Use<T>();
				
				c.For<If>().Transient().Use<T>();				
				c.For<If>().Transient().Add(new T());
				c.For<If>().Transient().Add<T>();

				c.For<If>().Use<T>();
				c.For<If>().Add<T>();
			});
	}

	interface If { }

	class T : If
	{
	}

	class K : T
	{
	}

	class N<M>
	{
	}
}");

			var wirings = analyzer.GetWirings();

			var grouped = wirings.GroupBy(x => x.Lifecycle).ToDictionary(x => x.Key, x => x.ToArray());
			
			Assert.Equal(15, grouped[Lifecycle.Singleton].Length);
			Assert.Equal(2, grouped[Lifecycle.Unique].Length);
			Assert.Equal(3, grouped[Lifecycle.Transient].Length);
			Assert.Equal(2, grouped[Lifecycle.TransientImplicit].Length);
		}
	}
}