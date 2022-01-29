using System;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using ClrHeapAllocationAnalyzer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;

namespace AllocationFree
{
    
    // Plan: 
    // 1. Properties to control whether we should (or how we should) analyze a given type (e.g. NoAllocation, ConstructorOnly, etc etc).
    // 2. Make any Analyzer warning in a property-enabled location a build error!
    // 3. Work out how to enable/disable the analysis both when building and when Roslyn is analysing.

    public class Class1
    {
        public void Foo()
        {
            var analyzer = new ExplicitAllocationAnalyzer();
        }
    }
}
