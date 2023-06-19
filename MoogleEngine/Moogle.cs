using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static TextProcessor textProcessor;

    public static SearchResult Query(string query)
    {
<<<<<<< HEAD
        string suggestion;
        SearchItem[] items = textProcessor.VectorialModel(query, out suggestion);
        return new SearchResult(items, suggestion);
=======
        SearchItem[] items = textProcessor.VectorialModel(query);
        return new SearchResult(items, query);
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
    }
}
