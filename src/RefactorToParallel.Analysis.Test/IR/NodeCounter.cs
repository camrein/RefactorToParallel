using RefactorToParallel.Analysis.IR;

namespace RefactorToParallel.Analysis.Test.IR {
  public static class InstructionCounter {
    public static int Count(Code code) {
      return code.Root.Count;
    }
  }
}
