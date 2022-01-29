using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AllocationFree
{
    // Lovingly re-appropriated from RoslynClrHeapAllocationAnalyzer's tests
    public class AllocationInfo
    {
        public CSharpParseOptions Options { get; set; }
        public SyntaxTree Tree { get; set; }
        public CSharpCompilation Compilation { get; set; }
        public ImmutableArray<Diagnostic> Diagnostics { get; set; }
        public SemanticModel SemanticModel { get; set; }
        public IList<SyntaxNode> Matches { get; set; }
        public List<Diagnostic> Allocations { get; set; }
    }
}