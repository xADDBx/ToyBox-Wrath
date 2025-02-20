namespace ToyBox.Infrastructure.Utilities;
public static class MiscExtensions {        
    // Credits to https://github.com/microsoftenator2022
    /// <summary>
    /// Divides input sequence into chunks of at most <paramref name="chunkSize"/>
    /// </summary>
    /// <returns>Sequence of sequences of <paramref name="chunkSize"/> elements. The last chunk will contain at most <paramref name="chunkSize"/> elements</returns>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize) {
        var chunk = new T[chunkSize];
        var i = 0;

        foreach (var element in source) {
            chunk[i] = element;

            i++;
            if (i == chunkSize) {
                yield return chunk;
                chunk = new T[chunkSize];
                i = 0;
            }
        }

        if (i > 0 && i < chunkSize) yield return chunk.Take(i);
    }
    public static string Format(this string s, object arg0) {
        return string.Format(s, arg0);
    }
    public static string Format(this string s, object arg0, object arg1) {
        return string.Format(s, arg0, arg1);
    }
    public static string Format(this string s, object arg0, object arg1, object arg2) {
        return string.Format(s, arg0, arg1, arg2);
    }
    public static string Format(this string s, params object[] args) {
        return string.Format(s, args);
    }
}
