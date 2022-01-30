# AllocationFree

This is very much an exploratory work in progress!

Note that until I find a better way to do this I have lovingly reappropriated soime analyzers from: https://github.com/microsoft/RoslynClrHeapAllocationAnalyzer.

# Building

Just to play around, I've been looking at how to include/exclude Roslyn analysis in a build

Running
```
dotnet build
```
will use Roslyn Analyzers when building  

```
dotnet build /p:UseRoslynAnalyzers=false 
```
will not (see Directory.build.props for details)
