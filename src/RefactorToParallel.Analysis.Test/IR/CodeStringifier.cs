using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Instructions;
using RefactorToParallel.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.IR {
  public class CodeStringifier : IInstructionVisitor<string> {
    private readonly Dictionary<Label, string> _labels = new Dictionary<Label, string>();
    
    private int _currentLabelId;

    private CodeStringifier() { }

    public static string Generate(Code code) {
      return new CodeStringifier()._Generate(code).Trim();
    }

    private string _Generate(Code code) {
      return string.Join("\r\n", code.Root.Select(instruction => instruction.Accept(this)));
    }

    public string Visit(Instruction instruction) {
      return instruction.Accept(this);
    }

    public string Visit(Assignment instruction) {
      return $"{instruction.Left} = {instruction.Right}";
    }

    public string Visit(Declaration instruction) {
      return $"DECL {instruction.Name}";
    }

    public string Visit(Label instruction) {
      return $"LABEL {_GetLabel(instruction)}";
    }

    public string Visit(Jump instruction) {
      return $"JUMP {_GetLabel(instruction.Target)}";
    }

    public string Visit(ConditionalJump instruction) {
      return $"IF {instruction.Condition} JUMP {_GetLabel(instruction.Target)}";
    }

    public string Visit(Invocation instruction) {
      return $"INVOKE {instruction.Expression}";
    }

    private string _GetLabel(Label instruction) {
      return _labels.GetOrCreate(instruction, () => $"#{_currentLabelId++}");
    }
  }
}
