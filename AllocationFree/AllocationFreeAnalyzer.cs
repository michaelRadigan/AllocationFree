using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AllocationFree.ClrAnalyzers;
using ClrHeapAllocationAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AllocationFree
{
    
    // Plan: 
    // 1. Properties to control whether we should (or how we should) analyze a given type (e.g. NoAllocation, ConstructorOnly, etc etc).
    // 2. Make any Analyzer warning in a property-enabled location a build error!
    // 3. Work out how to enable/disable the analysis both when building and when Roslyn is analysing.
    // 4. See if I can Wrap the existing HeapAnalyzer event source in some way to avoid having to change the underlying analyzer code from ClrHeapAnalyzer
    // 5. It would be very nice if the analyzers were just published in a way that allowed them to be easily imported.
    
    // Read this! :) https://docs.microsoft.com/en-us/archive/msdn-magazine/2014/special-issue/csharp-and-visual-basic-use-roslyn-to-write-a-live-code-analyzer-for-your-api 
    
    // So, the plan is that this Analyzer will wrap a collection of Analyzers. 
    // We should determine whether we are in a context that must be checked before delegating to our sub-analyzers
    
    
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllocationFreeAnalyzer : DiagnosticAnalyzer
    {

        private readonly AllocationAnalyzer[] _analyzers;
        private readonly SyntaxKind[] _expressions;

        private readonly Dictionary<SyntaxKind, HashSet<AllocationAnalyzer>> _syntaxKindMappings;
        

        public AllocationFreeAnalyzer()
        {
            // TODO[michaelr]: I'm hardcoding this with a single, simple analyzer for now as a proof of concept.
            _analyzers = new []{ new ExplicitAllocationAnalyzer() as AllocationAnalyzer,};
            
            _expressions = _analyzers.SelectMany(analyzer => analyzer.Expressions).Distinct().ToArray();
            
            // Ok, so we would also quite like to invert the mapping of analyzer -> Expression so that for each SyntaxKind that
            // our analyzers cover we have a set of analyzers to immediately reroute the call to.

            _syntaxKindMappings = new Dictionary<SyntaxKind, HashSet<AllocationAnalyzer>>();
            foreach (var analyzer in _analyzers)
            {
                foreach (var expression in analyzer.Expressions)
                {
                    if (_syntaxKindMappings.TryGetValue(expression, out var analyzerSet))
                    {
                        analyzerSet.Add(analyzer);
                    }
                    else
                    {
                        _syntaxKindMappings[expression] = new() {analyzer};
                    }
                }
            }
            
        }
        
        public override void Initialize(AnalysisContext context)
        {
            // I thi8nk that we would like to effectively man in the middle the registration of the allocation-checkers
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            // TODO[michaelr]: TO avoid any double counting do we need to check that the underlying Initialize methods of our Allocation Analyzers are not called?
            context.RegisterSyntaxNodeAction(Analyze, _expressions);
        }
        
        // Ok, so we are called back any time that a Syntax node matching the expression rules for any of our allocatin analyzers is matched.
        // We should determine whether we are in the context of requiring an AllocationFree check. If we are then we should 
        // forward on this node to the relevant analyzers. If we are not the the analysis gets cut off immediately.
        //
        // We should check whether we are in the context of requiring AllocationFree by climbing object hierarchy and looking
        // for the relevant attributes.
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

            var foo = context.Node.Kind();
            
            //AnalyzeNode(context);
        } 

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
    }
}
