using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test {
  /// <summary>
  /// Factory to generate IR code for test cases.
  /// </summary>
  public static class TestCodeFactory {
    /// <summary>
    /// Creates a code from the given source syntax.
    /// </summary>
    /// <returns></returns>
    public static Code CreateCode(string body) {
      var methodName = "Method";
      var code = $"class Test {{ void {methodName}() {{ {body} }} }}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      return CodeFactory.Create(method.Body);
    }

    /// <summary>
    /// Creates a three address code from the given source syntax.
    /// </summary>
    /// <returns></returns>
    public static Code CreateThreeAddressCode(string body) {
      return ThreeAddressCodeFactory.Create(CreateCode(body));
    }

    /// <summary>
    /// Creates an expression from the given source syntax.
    /// </summary>
    /// <returns></returns>
    public static Expression CreateExpression(string body) {
      var methodName = "Method";
      var code = $"class Test {{ void {methodName}() {{ var variable = {body}; }} }}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var expression = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => declarator.Initializer.Value)
        .Single();

      return new ExpressionFactory(null, false).Create(expression);
    }
  }
}
