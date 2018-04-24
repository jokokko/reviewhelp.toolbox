using Microsoft.CodeAnalysis.Diagnostics;

namespace ReviewHelp.Toolbox.Infrastructure
{
    public abstract class StructureMapAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(ctx =>
            {
                var structuremapCtx = new StructureMapContext(ctx.Compilation);

                if (ContextDefined(structuremapCtx))
                {
                    AnalyzeCompilation(ctx, structuremapCtx);
                }
            });
        }
        protected virtual bool ContextDefined(StructureMapContext structureMapCtx) => structureMapCtx.Version != null;
        protected abstract void AnalyzeCompilation(CompilationStartAnalysisContext ctx, StructureMapContext structureMapCtx);
    }
}