using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class TextProcessor
    {
        #region Fields
        Dictionary<string, string> documents;
        Terms terms;
        string[] queryTerms;
        float[,] tfidfMatrix;
        #endregion

        #region Constructor

        public TextProcessor()
        {
            // Crear una variable para guardar la ruta al fichero Content
            string path = Path.Combine(Directory.GetParent(".")!.ToString(), "Content");

            // Obtener los nombres de los documentos .txt del path
            string[] documentsNamesWithPath = documentsNamesWithPath = Directory.GetFiles(path, "*txt", SearchOption.AllDirectories);

            // Guardar los nombres (key) y su correspondiente texto (value) de los documentos .txt en un diccionario 
            documents = new Dictionary<string, string>();
            for (int i = 0; i < documentsNamesWithPath.Length; i++)
                documents.Add(Path.GetFileNameWithoutExtension(documentsNamesWithPath[i]), File.ReadAllText(documentsNamesWithPath[i]));

            // Procesar los documentos y crear la matrix de TF-IDF
            terms = new Terms(documents);
            tfidfMatrix = terms.GetTFIDFMatrix();

        }

        #endregion

        #region Search
        // Metodo que devuelve los resultados del query
        public SearchItem[] VectorialModel(string query)
        {
            // Se obtienen el vector de la quers, sus palabras normalizadas
            float[] queryVector = terms.GetQueryVector(query, out queryTerms);

            // Se calcula el score a traves de la similitud de coseno y se obtiene el resultado de la busqueda
            float[] cosineSimilarity = CosineSimilarity(tfidfMatrix, queryVector);
            SearchItem[] queryDocumentsResult = QueryDocumentsResult();

            // Se ordena en orden descente el resultado de la busqueda y se devuelve
            Sort(queryDocumentsResult);
            return queryDocumentsResult;

            // Metodo auxiliar que relaciona los documentos con el score y busca el snippet dentro del documento
            SearchItem[] QueryDocumentsResult()
            {
                // Crear un array temporal para los resultados de la busqueda
                SearchItem[] tempResult = new SearchItem[documents.Count];

                // Obtener los snpitted de todos los documentos con respecto al query
                string[] snippets = GetSnippets();

                // Iterar por los documentos 
                int indexResult = 0, indexDocument = 0;
                foreach (var title in documents.Keys)
                {
                    // Verificar el snippet y el score: no se devolveran los documentos con score 0 y que su spippet sea null
                    if (!string.IsNullOrEmpty(snippets[indexDocument]) && cosineSimilarity[indexDocument] != 0)
                        tempResult[indexResult++] = new SearchItem(title, snippets[indexDocument], cosineSimilarity[indexDocument]);

                    indexDocument++;
                }

                return tempResult[0..indexResult];
            }
        }

        // Metodo que calcula la similitud de cosino de los documentos
        private float[] CosineSimilarity(float[,] tfidfMatrix, float[] queryVector)
        {
            // Calcular el vector resultante de la multiplicacion de la matriz de TF-IDF con 
            float[] vectorResultante = Matrix.Multiplication(tfidfMatrix, queryVector);
            
            // Calcular la norma
            float[] normaMatrix = Matrix.Norma(tfidfMatrix);
            float normaQuery = Vector.Norma(queryVector);

            // Calcular la similutd de coseno
            float[] vectorSimilitud = new float[documents.Count];
            for (int i = 0; i < documents.Count; i++)
            {
                vectorSimilitud[i] = vectorResultante[i] / (normaMatrix[i] * normaQuery);
            }

            return vectorSimilitud;
        }

        // Metodo que ordena los documentos resultantes de la query (Selection sort invertido)
        private void Sort(SearchItem[] queryDocumentsResult)
        {
            for (int i = 0; i < queryDocumentsResult.Length; i++)
            {
                for (int j = i + 1; j < queryDocumentsResult.Length; j++)
                {
                    if (queryDocumentsResult[j].Score > queryDocumentsResult[i].Score)
                    {
                        SearchItem temp = queryDocumentsResult[i];
                        queryDocumentsResult[i] = queryDocumentsResult[j];
                        queryDocumentsResult[j] = temp;
                    }
                }
            }
        }

        #endregion

        #region Snippet

        // Metodo general
        private string[] GetSnippets()
        {
            // Crear una variable donde guardar los snippets 
            string[] snippets = new string[documents.Count];

            // Iterar por los documentos obtieniendo el snippet correspondiente
            int index = 0;
            foreach (var title in documents.Keys)
            {
                snippets[index] = SnippetScore(title, documents[title], index);
                index++;
            }

            return snippets;
        }

        // Metodo que devuelve el snippet de un documento con mayor score
        private string SnippetScore(string title, string text, int indexDocument)
        {
            /* Diccionario que trendra como
                - Key: las posiociones de un termino de la query
                - Value: el tfidf del respectivo termino  */
            Dictionary<int, float> queryPositions = new Dictionary<int, float>();

            /* Diccionario que tendra como
                - Key: el tfidf de los terminos de la query
                - Value: el numero de veces que aparece el termino en el snippet  */
            Dictionary<float, int> queryTFIDFValues = new Dictionary<float, int>();

            // Iterar por los terminos de la query para rellenar los diccionario
            for (int i = 0; i < queryTerms.Length; i++)
            {
                // Obtener el TF-IDF del termino y agregarlo al diccionario
                float tfidfValue = tfidfMatrix[indexDocument, terms.GetTermIndex(queryTerms[i])];

                if (tfidfValue == 0)
                    continue;

                queryTFIDFValues.Add(tfidfValue, 0);

                // Obtener las posiciones de los terminos de la query en el documento y los agrega al diccionario  
                int[] positions = terms.GetPositions(title, queryTerms[i]);
                for (int j = 0; j < positions.Length; j++)
                {
                    queryPositions.Add(positions[j], tfidfValue);
                }
            }

            // Caso especifico cuando en el documento no aparece ningun termino de la query
            if (queryPositions.Count == 0)
                return "";

            // Crear un array con las posiciones de los terminos ordenadas
            int[] queryPositionsSorted = (from position in queryPositions.Keys orderby position select position).ToArray();
            
            // Fijar el tamaño de caracteres que debe tener el snippet
            int snippetlength = 200;

            // Crear una tupla que guader el snippet con mayor score junto con la poscion en donde empieza y termina
            (int start, int end, float score) snippet = (0, 0, 0);

            // Itera por las posicones calculando el mayor score
            for (int startIndex = 0, finalIndex = 0; finalIndex < queryPositionsSorted.Length; finalIndex++)
            {
                // Aumentar el numero de veces que aparece un termino en un snippet determinado por startIndex y finalInadex
                queryTFIDFValues[queryPositions[queryPositionsSorted[finalIndex]]]++;

                // Comprobar que las posciones sean menores que snippetlength
                if (finalIndex == queryPositionsSorted.Length - 1 || queryPositionsSorted[finalIndex + 1] - queryPositionsSorted[startIndex] > snippetlength)
                {
                    // Crear un score temporal
                    float scoreTemp = 0;

                    // Declarar una variable que lleva el numero de terminos que hay dentro del snippet 
                    int queryCount = 0;

                    // Iterar por el diccionario para calcular el score temporal
                    foreach (var key in queryTFIDFValues.Keys)
                    {
                        queryCount += (queryTFIDFValues[key] > 0) ? 1 : 0;
                        scoreTemp += key * queryTFIDFValues[key];

                        queryTFIDFValues[key] = 0;
                    }

                    scoreTemp *= queryCount * queryCount;

                    // Comprobrar si el score temporal es mayor que el actual
                    if (scoreTemp > snippet.score)
                        snippet = (queryPositionsSorted[startIndex], queryPositionsSorted[finalIndex], scoreTemp);

                    startIndex = finalIndex + 1;
                }

            }

            return GetSnippetFromDocument(text, snippet.start, snippet.end);

        }

        // Metodo que obtiene el snippet del documento
        private static string GetSnippetFromDocument(string text, int start, int end)
        {
            // Declarar dos boolenos para controlar cuando termina la linea
            bool notStartLine = true, notEndLine = true;

            // Crear un contador para la longitud del snippet a partir de la posicion start y end
            int counter = 0;

            // Iterar por el texto del documento
            while ((notStartLine || notEndLine) && counter != 100)
            {
                // Aumentar el contador
                counter++;

                // Determinar si la posicion start esta al principio de linea
                if (notStartLine && start >= 0 && text[start] != '\n')
                    start--;
                else
                    notStartLine = false;

                // Determinar si la posicion end esta al final de linea
                if (notEndLine && end < text.Length && text[end] != '\n')
                    end++;
                else
                    notEndLine = false;

            }

            // Crear el snippet 
            StringBuilder snippet = new StringBuilder();
            for (int i = start + 1; i < end; i++)
            {
                snippet.Append(text[i]);
            }

            return snippet.ToString();

        }

        #endregion
    }
}
