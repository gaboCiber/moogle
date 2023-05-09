using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static TextProcessor textProcessor;

    public static SearchResult Query(string query)
    {
        SearchItem[] items = textProcessor.VectorialModel(query);
        return new SearchResult(items, query);
    }
}
