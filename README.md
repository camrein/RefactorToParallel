# RefactorToParallel
Sourcecode of the prototype of the Master's Thesis Automatic Refactoring for Parallelization

# Implementation notes

The prototype is a static code analyzer available as a Visual Studio<sup>1</sup> Plugin as well as a NuGet<sup>2</sup> package. Its only dependency is the .NET Compiler Platform (Roslyn)<sup>3</sup> which is used analyze arbitrary C\# code. The Visual Studio integration allows just-in-time code analysis and automatically reports `for` loops that can be refactored to their `Parallel.For`<sup>4</sup> counterpart safely. The following screenshot illustrates how the prototype informs about the parallelization opportunity including a preview of the code changes.

![Visual Studio screenshot illustrating the
plugin suggesting the parallelization opportunity](images/refactor_suggestion.png)

In the upcoming Section, the steps the prototype uses for its analysis are explained.

## Analysis Steps

## Sources

1. [Visual Studio](https://www.visualstudio.com)
2. [NuGet](https://www.nuget.org/)
3. [.NET Compiler Platform (Roslyn)](https://github.com/dotnet/roslyn)
4. [Parallel.For Method](https://msdn.microsoft.com/en-us/library/system.threading.tasks.parallel.for(v=vs.110).aspx)

