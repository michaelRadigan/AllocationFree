using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace AllocationFree.Test
{
    public class TestAllocationFreeAnalyzer
    {
        [Test]
        public void ClassWithAllocationFreeAttributeAnalyzedCorrectly()
        {
            var sampleProgram =
                @"using System;

[AllocationFree.AllocationFree]
public class TestClass
{
    public string Name { get; set; }
    public int[] Array = new [] {0, 1, 2}; 
}";
            var analyzer = new AllocationFreeAnalyzer();
            var info = AnalyzerRunner.ProcessCode(analyzer, sampleProgram, ImmutableArray.Create(SyntaxKind.AnonymousObjectCreationExpression));
            Console.WriteLine(info.Allocations.Count);
            Assert.That(info.Allocations.Count, Is.EqualTo(1));
        }
    }
}