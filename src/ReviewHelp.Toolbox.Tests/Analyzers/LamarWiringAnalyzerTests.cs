using System.Linq;
using ReviewHelp.Toolbox.Analyzers;
using ReviewHelp.Toolbox.Model;
using ReviewHelp.Toolbox.Tests.Infrastructure;
using Xunit;

namespace ReviewHelp.Toolbox.Tests.Analyzers
{
	public sealed class LamarWiringAnalyzerTests
	{
		[Fact]
		public async void CanDetermineWirings()
		{
			var analyzer = new LamarWiringAnalyzer();
			await TestHelper.GetDiagnosticsAsync(analyzer,
				@"using Lamar;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

	class TestClass
	{
		void TestMethod()
		{
			new Container(c =>
			{					
				c.ForSingletonOf<If>().Use<T>();				
				c.For<If>().Use<T>().Singleton();
				c.For<If>().Use<T>().Scoped();
				c.For<If>().Use<T>().Transient();
				c.For<If>().Add<T>();
				c.For(typeof(If)).Use(typeof(T));
				c.For<If>().Add(new K());
				c.For<If>().Add(context => new K());
				var k = new K();
				c.For<If>().Add(context => k);
				var registration = c.For<If>().Use<T>();				
				registration.Lifetime = ServiceLifetime.Singleton;				
				ConstructorInstance<T> registration2;
				registration2 = c.For<If>().Use<T>();				
				registration2.Lifetime = ServiceLifetime.Scoped;				
			});
		}

		interface If { }

		class T : If
		{
		}

		class K : T
		{
		}
}");

			var wirings = analyzer.GetWirings();

			var grouped = wirings.GroupBy(x => x.Lifecycle).ToDictionary(x => x.Key, x => x.ToArray());
			
			Assert.Equal(3, grouped[LamarLifecycle.Singleton].Length);
			Assert.Equal(1, grouped[LamarLifecycle.Transient].Length);
			Assert.Equal(2, grouped[LamarLifecycle.Scoped].Length);
			Assert.Equal(5, grouped[LamarLifecycle.TransientImplicit].Length);			
		}
	}
}