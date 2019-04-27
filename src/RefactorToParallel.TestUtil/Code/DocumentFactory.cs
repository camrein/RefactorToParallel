using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace RefactorToParallel.TestUtil.Code {
  /// <summary>
  /// This factory is used to create documents out of given sources.
  /// </summary>
  public class DocumentFactory {
    private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
    private static readonly MetadataReference SystemReference = MetadataReference.CreateFromFile(typeof(Semaphore).Assembly.Location);
    private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
    //private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
    //private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

    /// <summary>
    /// Gets the project used.
    /// </summary>
    public Project Project { get; private set; }

    private DocumentFactory(Project project) {
      Project = project;
    }

    /// <summary>
    /// Creates a new instance of the factory.
    /// </summary>
    /// <returns>The created document factory.</returns>
    public static DocumentFactory Create() {
      var workspace = new AdhocWorkspace();
      var project = workspace.AddProject("Unit Test", "C#")
        .AddMetadataReferences(new[] { CorlibReference, SystemReference, SystemCoreReference/*, CSharpSymbolsReference, CodeAnalysisReference*/ });
      workspace.TryApplyChanges(project.Solution);
      return new DocumentFactory(project);
    }

    /// <summary>
    /// Creates a document of the given source. The document will have the filename "Test.cs".
    /// </summary>
    /// <param name="source">Source to create the document of.</param>
    /// <returns>The created document.</returns>
    public Document CreateFromSource(string source) {
      var document = Project.AddDocument("Test.cs", source);
      Project = document.Project;
      return document;
    }

    /// <summary>
    /// Creates a semantic model of the given source. The document will have the filename "Test.cs".
    /// </summary>
    /// <param name="source">Source to create the semantic model of.</param>
    /// <returns>The created semantic model.</returns>
    public SemanticModel CreateSemanticModel(string source) {
      return CreateFromSource(source).GetSemanticModelAsync().Result;
    }
  }
}
