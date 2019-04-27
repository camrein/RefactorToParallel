
namespace RefactorIntegrationTest {
  public class Search {
    public bool Contains<T>(T searched, T[] data) {
      for(var i = 0; i < data.Length; ++i) {
        if(searched.Equals(data[i])) {
          return true;
        }
      }

      return false;
    }

    public int Count<T>(T searched, T[] data) {
      var count = 0;
      for(var i = 0; i < data.Length; ++i) {
        if(searched.Equals(data[i])) {
          ++count;
        }
      }

      return count;
    }
  }
}
