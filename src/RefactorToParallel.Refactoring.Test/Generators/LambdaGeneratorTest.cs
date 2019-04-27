using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Refactoring.Generators;

namespace RefactorToParallel.Refactoring.Test.Generators {
  [TestClass]
  public class LambdaGeneratorTest {
    [TestMethod]
    public void CreateLambdaWithSingleInvocationAndNoArgument() {
      var identifier = SyntaxFactory.Identifier("i");
      var body = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("Console"),
          SyntaxFactory.IdentifierName("WriteLine")
        )
      ));

      Assert.AreEqual("i=>{Console.WriteLine();}", LambdaGenerator.CreateLambdaOrDelegate(identifier, body).ToString());
    }

    [TestMethod]
    public void CreateLambdaWithSingleInvocationAndConstantArgument() {
      var identifier = SyntaxFactory.Identifier("j");
      var body = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("Console"),
          SyntaxFactory.IdentifierName("WriteLine")
        ),
        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("hello")))))
      ));

      Assert.AreEqual(@"j=>{Console.WriteLine(""hello"");}", LambdaGenerator.CreateLambdaOrDelegate(identifier, body).ToString());
    }

    [TestMethod]
    public void CreateLambdaWithSingleInvocationAndVariableArgument() {
      var identifier = SyntaxFactory.Identifier("j");
      var body = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("Console"),
          SyntaxFactory.IdentifierName("WriteLine")
        ),
        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("b")))))
      ));

      Assert.AreEqual("j=>{Console.WriteLine(b);}", LambdaGenerator.CreateLambdaOrDelegate(identifier, body).ToString());
    }

    [TestMethod]
    public void CreateLambdaWithSingleInvocationAndLambdaArgument() {
      var identifier = SyntaxFactory.Identifier("i");
      var body = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("Console"),
          SyntaxFactory.IdentifierName("WriteLine")
        ),
        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier))))
      ));

      Assert.AreEqual("Console.WriteLine", LambdaGenerator.CreateLambdaOrDelegate(identifier, body).ToString());
    }

    [TestMethod]
    public void CreateLambdaWithSingleInvocationBlockAndLambdaArgument() {
      var identifier = SyntaxFactory.Identifier("i");
      var body = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("Debug"),
          SyntaxFactory.IdentifierName("WriteLine")
        ),
        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier))))
      )));

      Assert.AreEqual("Debug.WriteLine", LambdaGenerator.CreateLambdaOrDelegate(identifier, body).ToString());
    }

    [TestMethod]
    public void CreateLambdaWitDoubleInvocationBlockAndLambdaArgument() {
      var identifier = SyntaxFactory.Identifier("i");
      var invocation = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("Console"),
          SyntaxFactory.IdentifierName("WriteLine")
        ),
        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier))))
      ), SyntaxFactory.Token(SyntaxKind.SemicolonToken));

      var body = SyntaxFactory.Block(invocation, invocation);

      Assert.AreEqual("i=>{Console.WriteLine(i);Console.WriteLine(i);}", LambdaGenerator.CreateLambdaOrDelegate(identifier, body).ToString());
    }
  }
}
