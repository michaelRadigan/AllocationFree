using System.Linq;
using ClrHeapAllocationAnalyzer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AllocationFree.ClrAnalyzers
{
    public abstract class AllocationAnalyzer : DiagnosticAnalyzer
    {
        // TODO[michaelr]: Note that I've changed this to public
        public abstract SyntaxKind[] Expressions { get; }

        public abstract void AnalyzeNode(SyntaxNodeAnalysisContext context);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Analyze, Expressions);
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (AllocationRules.IsIgnoredFile(context.Node.SyntaxTree.FilePath))
            {
                return;
            }

            if (context.ContainingSymbol.GetAttributes().Any(AllocationRules.IsIgnoredAttribute))
            {
                return;
            }

            AnalyzeNode(context);
        }
    }
}