using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static TextProcessor textProcessor;

    public static SearchResult Query(string query)
    {
        string suggestion;
        SearchItem[] items = textProcessor.VectorialModel(query, out suggestion);
        return new SearchResult(items, suggestion);
    }
}
