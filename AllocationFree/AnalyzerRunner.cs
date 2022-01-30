using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace AllocationFree
{
    // Lovingly re-appropriated from RoslynClrHeapAllocationAnalyzer's tests
    public class AnalyzerRunner
    {
         public static readonly List<MetadataReference> References = new()
         {
                MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IList<>).Assembly.Location),
                // Note that this is important and things will just entirely fail without it!
                MetadataReference.CreateFromFile(typeof(AllocationFree).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            };

        public static IList<SyntaxNode> GetExpectedDescendants(IEnumerable<SyntaxNode> nodes, ImmutableArray<SyntaxKind> expected)
        {
            var descendants = new List<SyntaxNode>();
            foreach (var node in nodes)
            {
                if (expected.Any(e => e == node.Kind()))
                {
                    descendants.Add(node);
                    continue;
                }

                foreach (var child in node.ChildNodes())
                {
                    if (expected.Any(e => e == child.Kind()))
                    {
                        descendants.Add(child);
                        continue;
                    }

                    if (child.ChildNodes().Count() > 0)
                        descendants.AddRange(GetExpectedDescendants(child.ChildNodes(), expected));
                }
            }
            return descendants;
        }

        public static AllocationInfo ProcessCode(DiagnosticAnalyzer analyzer, string sampleProgram,
            ImmutableArray<SyntaxKind> expected, bool allowBuildErrors = false, string filePath = "")
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Script);
            var tree = CSharpSyntaxTree.ParseText(sampleProgram, options, filePath);
            var compilation = CSharpCompilation.Create("Test", new[] { tree }, References);

            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error) > 0)
            {
                var msg = "There were Errors in the sample code\n";
                if (allowBuildErrors == false)
                    Assert.Fail(msg + string.Join("\n", diagnostics));
                else
                    Console.WriteLine(msg + string.Join("\n", diagnostics));
            }

            var semanticModel = compilation.GetSemanticModel(tree);
            var matches = GetExpectedDescendants(tree.GetRoot().ChildNodes(), expected);

            // Run the code tree through the analyzer and record the allocations it reports
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
            var allocations = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().Distinct(DiagnosticEqualityComparer.Instance).ToList();

            return new AllocationInfo
            {
                Options = options,
                Tree = tree,
                Compilation = compilation,
                Diagnostics = diagnostics,
                SemanticModel = semanticModel,
                Matches = matches,
                Allocations = allocations,
            };
        }
    }
    
    internal class DiagnosticEqualityComparer : IEqualityComparer<Diagnostic>
    {
        public static DiagnosticEqualityComparer Instance = new DiagnosticEqualityComparer();

        public bool Equals(Diagnostic x, Diagnostic y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Diagnostic obj)
        {
            return Combine(obj?.Descriptor.GetHashCode(),
                Combine(obj?.GetMessage().GetHashCode(),
                    Combine(obj?.Location.GetHashCode(),
                        Combine(obj?.Severity.GetHashCode(), obj?.WarningLevel)
                    )));
        }

        internal static int Combine(int? newKeyPart, int? currentKey)
        {
            int hash = unchecked(currentKey.Value * (int)0xA5555529);

            if (newKeyPart.HasValue)
            {
                return unchecked(hash + newKeyPart.Value);
            }

            return hash;
        }
    }
}