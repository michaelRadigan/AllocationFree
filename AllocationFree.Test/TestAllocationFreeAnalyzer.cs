using System;
using System.Collections.Immutable;
using ClrHeapAllocationAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace AllocationFree.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var sampleProgram =
                @"using System;
var temp = new { A = 123, Name = ""Test"", };";
            var analyser = new ExplicitAllocationAnalyzer();
            var info = AnalyzerRunner.ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.AnonymousObjectCreationExpression));
            Console.WriteLine(info.Allocations.Count);
            Assert.That(info.Allocations.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void Test2()
        {
            var sampleProgram =
                @"using System;
using AllocationFree;

[AllocationFree]
public class TestClass
{
    public string Name { get; set; }
}";
            var analyser = new ExplicitAllocationAnalyzer();
            var info = AnalyzerRunner.ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.AnonymousObjectCreationExpression));
            Console.WriteLine(info.Allocations.Count);
            Assert.That(info.Allocations.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void Test3()
        {
            var sampleProgram =
                @"using System;

[AllocationFree.AllocationFree]
public class TestClass
{
    public string Name { get; set; }
    public int[] Array = new [] {0, 1, 2}; 
}";
            var analyser = new ExplicitAllocationAnalyzer();
            var info = AnalyzerRunner.ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.AnonymousObjectCreationExpression));
            Console.WriteLine(info.Allocations.Count);
            //Assert.That(info.Allocations.Count, Is.EqualTo(1));
        }
    }
}