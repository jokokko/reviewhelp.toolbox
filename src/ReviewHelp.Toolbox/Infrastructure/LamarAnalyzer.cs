using Microsoft.CodeAnalysis.Diagnostics;

namespace ReviewHelp.Toolbox.Infrastructure
{
	public abstract class LamarAnalyzer : DiagnosticAnalyzer
	{
		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

			context.RegisterCompilationStartAction(ctx =>
			{
				var lamarCtx = new LamarContext(ctx.Compilation);

				if (ContextDefined(lamarCtx))
				{
					AnalyzeCompilation(ctx, lamarCtx);
				}
			});
		}
		protected virtual bool ContextDefined(LamarContext lamarCtx) => lamarCtx.Version != null;
		protected abstract void AnalyzeCompilation(CompilationStartAnalysisContext ctx, LamarContext lamarCtx);
	}
}