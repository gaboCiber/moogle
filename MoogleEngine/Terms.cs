using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Terms
    {
        #region Fields

        // Una matrix de tipo float que guardara el TF-IDF
        float[,] tfidfMatrix;

<<<<<<< HEAD
        // Numero de palabras normalizadas (sin repetir) que tienen en general los documentos
        int normalizedTermsCount;       
=======
        // Lista que guardara las palabras normalizados de todos los documentos (sin repetir)
        List<string> termsNormalized;
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5

        /* Diccionario que tiene como
           - Key: los nombres de los documentos .txt
           - Value: el texto de cada documentos .txt  */
        Dictionary<string, string> documents;

        /* Diccionario que tiene como
           - Key: las palabras normalizados de todos los documentos (sin repetir)
<<<<<<< HEAD
           - Value: el numero de documentos donde aparece la palabra y el indece que tiene la palabra dentro del diccionario  */
        Dictionary<string, (int frecuency, int index)> termsFrequencyPerDocument;
=======
           - Value: el numero de documentos donde aparece la palabra  */
        Dictionary<string, int> termsFrequencyPerDocument;
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5

        /* Diccionario que tiene como
           - Key: los nombres de los documentos .txt
           - Value: la frecuenca maxima de palabras de cada documento   */
        Dictionary<string, int> maxTermsFrequencyOfEachDocument;

        /* Diccionario que tiene como
           - Key: los nombres de los documentos .txt
           - Value: otro diccionario que tiene como
                ~ SubKey: las palabras normalizadas del documento correspondiente de la key
                ~ SubValue: una lista con las posiciones donde aparece la palabra (de la subkey) dentro del documento */
        Dictionary<string, Dictionary<string, List<int>>> termsLineAndFrequency;
        
        #endregion

        #region Constructor

        public Terms(Dictionary<string, string> documents)
        {
            // Initialize the dictionaries
            this.documents = documents;
<<<<<<< HEAD
            normalizedTermsCount = 0;
            termsLineAndFrequency = new Dictionary<string, Dictionary<string, List<int>>>(); // Position 0: line | Position 1: frequency
            termsFrequencyPerDocument = new Dictionary<string, (int frecuency, int index)>();
=======
            termsNormalized = new List<string>();
            termsLineAndFrequency = new Dictionary<string, Dictionary<string, List<int>>>(); // Position 0: line | Position 1: frequency
            termsFrequencyPerDocument = new Dictionary<string, int>();
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
            maxTermsFrequencyOfEachDocument = new Dictionary<string, int>();

            // Get their keys and Values
            NormalizeAndFrequency();

            // Calculate the TF-IDF Matrix
<<<<<<< HEAD
            tfidfMatrix = new float[documents.Count, termsFrequencyPerDocument.Count];
=======
            tfidfMatrix = new float[documents.Count, termsNormalized.Count];
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
            CalculeteTFIDFMatrix();

        }

        #endregion

        #region Normalize and Frequency

        // Metodo que obtendra los key y values de los diccionarios
        private void NormalizeAndFrequency()
        {
            // Iterar sobre los documentos
            foreach (var title in documents.Keys)
            {
                // Normalizar, obtener las frecuencias y llenar los diccionarios
                maxTermsFrequencyOfEachDocument.Add(title, 0);
                termsLineAndFrequency.Add(title, Normalizer(documents[title]));

                // Caso especifico cuando se procesa un documento vacio
                if (termsLineAndFrequency[title].Count == 0)
                {
                    documents.Remove(title);
                    termsLineAndFrequency.Remove(title);
                    maxTermsFrequencyOfEachDocument.Remove(title);
                }
            }
<<<<<<< HEAD
        }

        // Metodo que normaliza y obtiene las diferentes frecuencias de un documento
        private Dictionary<string, List<int>> Normalizer(string text, bool isQuery = false, List<(char operador, int termIndex)> queryOperator = null)
=======

            // Obtener las palabras normalizadas
            termsNormalized = termsFrequencyPerDocument.Keys.ToList();
        }

        // Metodo que normaliza y obtiene las diferentes frecuencias de un documento
        private Dictionary<string, List<int>> Normalizer(string text)
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
        {
            // Crear el diccionario que sera devuelto por el metodo
            Dictionary<string, List<int>> terms = new Dictionary<string, List<int>>();
            
            // Variable que guardara una palabra normalizada a medida que se procesa el texto
            StringBuilder actualWord = new StringBuilder();

<<<<<<< HEAD
            // Variable que se sera utilizada cuando en la query aparezca el operador de cercania "~"
            bool isCloserOperator = false;

=======
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
            // Iterar sobre la variable text y determinar si un caracter es una letra o un numero
            int i;
            for (i = 0; i < text.Length; i++)
            {
<<<<<<< HEAD
                if (!char.IsLetterOrDigit(text[i]))
                {
                    // Comprobar si es un operador
                    if (isQuery) 
                    {
                        switch (text[i])
                        {
                            case '*' or '!' or '^':
                                queryOperator.Add((text[i], terms.Count));
                                break;
                            case '~':
                                if(!isCloserOperator && terms.Count != 0)
                                    isCloserOperator = true;
                                break;
                        }
                    }

=======
                // 
                if (!char.IsLetterOrDigit(text[i]))
                {
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
                    Check(actualWord, i);
                    continue;
                }

                char caracter = ' ';

                if (char.IsDigit(text[i]))
                {
                    caracter = text[i];
                }
                else
                {
                    switch (text[i])
                    {
                        case 'á' or 'Á':
                            caracter = 'a';
                            break;
                        case 'é' or 'É':
                            caracter = 'e';
                            break;
                        case 'í' or 'Í':
                            caracter = 'i';
                            break;
                        case 'ó' or 'Ó':
                            caracter = 'o';
                            break;
                        case 'ú' or 'Ú':
                            caracter = 'u';
                            break;
                        case 'Ñ':
                            caracter = 'ñ';
                            break;
                        default:
                            caracter = Convert.ToChar(text[i].ToString().ToLower());
                            break;
                    }
                }

                actualWord.Append(caracter);
            }

            Check(actualWord, i);

            return terms;

            // Metodo auxiliar que llenarara los diferentes diccionario cuando un caracter no sea una letra o un numero
            void Check(StringBuilder actualWord, int i)
            {
                string word = actualWord.ToString();
                if (!string.IsNullOrWhiteSpace(word))
                {
<<<<<<< HEAD
                    if (!terms.ContainsKey(word)) // Para termsLineAndFrequency
                    {
                        terms.Add(word, new List<int>());

                        if (!isQuery)
                        {
                            if (termsFrequencyPerDocument.ContainsKey(word))  // Para termsFrequencyPerDocument
                                termsFrequencyPerDocument[word] = (termsFrequencyPerDocument[word].frecuency + 1, termsFrequencyPerDocument[word].index);
                            else
                            {
                                termsFrequencyPerDocument.Add(word, (1, normalizedTermsCount));
                                normalizedTermsCount++;
                            } 
                        }
=======
                    // Para termsLineAndFrequency
                    if (!terms.ContainsKey(word))
                    {
                        terms.Add(word, new List<int>());

                        // Para termsFrequencyPerDocument
                        if (termsFrequencyPerDocument.ContainsKey(word))
                        {
                            termsFrequencyPerDocument[word]++;
                        }
                        else
                        {
                            termsFrequencyPerDocument.Add(word, 1);
                        }

>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
                    }

                    terms[word].Add(i);

<<<<<<< HEAD
                    if (!isQuery && maxTermsFrequencyOfEachDocument.Values.Last() < terms[word].Count)  // Para maxTermsFrequencyOfEachDocument
                        maxTermsFrequencyOfEachDocument[maxTermsFrequencyOfEachDocument.Keys.Last()] = terms[word].Count;

                    if(isQuery && isCloserOperator)  // Para el operador de cercania
                    {
                        queryOperator.Add(('~', terms.Count - 1));
                        isCloserOperator = false;
                    }

=======
                    // Para maxTermsFrequencyOfEachDocument
                    if (maxTermsFrequencyOfEachDocument.Values.Last() < terms[word].Count)
                        maxTermsFrequencyOfEachDocument[maxTermsFrequencyOfEachDocument.Keys.Last()] = terms[word].Count;

>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
                    actualWord.Clear();
                }
            }

        }

        #endregion

        #region Get Values

        // Devuelve el indice de una palabra que se encuentra en la lista termsNormalized
        public int GetTermIndex(string term)
        {
<<<<<<< HEAD
            if (termsFrequencyPerDocument.ContainsKey(term))
                return termsFrequencyPerDocument[term].index;
            else
                return -1;
=======
            return termsNormalized.IndexOf(term);
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
        }

        // Devuelve la frecuencia de una palabra de un documento
        public int GetFrequency(string title, string term)
        {
            if (termsLineAndFrequency[title].ContainsKey(term))
                return termsLineAndFrequency[title][term].Count;
            else
                return 0;
        }

        // Devuelve como un array la lista de las posiones de una palabra en un documento
        public int[] GetPositions(string title, string term)
        {
            if (termsLineAndFrequency[title].ContainsKey(term))
                return termsLineAndFrequency[title][term].ToArray();
            else
                return new int[0];
        }

        // Devuelve la matrix de TFIDF
        public float[,] GetTFIDFMatrix()
        {
            return tfidfMatrix;
        }

        #endregion

        #region Matrix and Vector

        // Calcular la matriz de TF-IDF
        public void CalculeteTFIDFMatrix()
        {
            int i = 0, j = 0;
            foreach (var title in documents.Keys)
            {
<<<<<<< HEAD
                foreach (var term in termsFrequencyPerDocument.Keys)
                {
                    float num = ( GetFrequency(title, term) / (float) maxTermsFrequencyOfEachDocument[title]) * MathF.Log10( documents.Count / (float)  termsFrequencyPerDocument[term].frecuency);

=======
                foreach (var term in termsNormalized)
                {
                    float num = CalculateTF(GetFrequency(title, term), maxTermsFrequencyOfEachDocument[title]) * CalculateIDF(documents.Count, termsFrequencyPerDocument[term]);
                    
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
                    tfidfMatrix[i, j] = num;
                    j++;
                }
                i++;
                j = 0;
            }

            static float CalculateTF(float frequency, float maxfrequency) => frequency / maxfrequency;

            static float CalculateIDF(float totalDocuments, float termPerDocuments) => MathF.Log10(totalDocuments / termPerDocuments);
        }

        // Devuelve el vector correspondiente a la query
<<<<<<< HEAD
        public float[] GetQueryVector(string query, out string[] queryTerms, out List<(char operador, int termIndex)> queryOperator, out string suggestion)
        {
            // Crear una lista para guardar los operadore de la query con el indice de la palabra correspondiente
            queryOperator = new List<(char operador, int termIndex)>();

            // Crear una lista con las palabras normalizadas de la query y el vector correspondiente a la query relacionado con la matriz de TD-IDF
            Dictionary<string, List<int>> queryTermsList = Normalizer(query, true, queryOperator);                    

            float[] queryVector = new float[termsFrequencyPerDocument.Count];

            // Varables que guardaran la posible sugerencia y si es necesaria mostrarla en la pagina wb
            StringBuilder suggestionSB = new StringBuilder();
            bool needForSuggestion = false;

            // Iterar por la lista para rellenar el vector de la query
            for (int i = 0; i < queryTermsList.Count; i++)
            {
                string actualQueryTerm = queryTermsList.Keys.ElementAt(i);

                // Verificar si las palabras de la query estan alguno de los documentos
                if (termsFrequencyPerDocument.ContainsKey(actualQueryTerm))
                {
                    queryVector[GetTermIndex(actualQueryTerm)] = queryTermsList[actualQueryTerm].Count;
                    suggestionSB.Append(actualQueryTerm);
                }
                else
                {
                    // Obtener del universo de palabras de la query la mas parecida a la palabra de la query 
                    suggestionSB.Append(Suggestion(actualQueryTerm));
                    needForSuggestion = true;

                    // Eliminar los posibles operadores que haya tenido la palabra 
                    for (int j = 0; j < queryOperator.Count; j++)
                    {
                        if (i == queryOperator[j].termIndex || (queryOperator[j].operador == '~' && i == queryOperator[j].termIndex - 1))
                        {
                            queryOperator.RemoveAt(j);
                            j--;
                        }
                    }

                    // Eliminar la palabra de la query 
                    queryTermsList.Remove(actualQueryTerm);
                    i--;
                }

                suggestionSB.Append(" ");                    
            }

            // Devolver el vector de la query y sus palabras normalizadas
            queryTerms = queryTermsList.Keys.ToArray();
            suggestion = (needForSuggestion) ? suggestionSB.ToString() : "";

=======
        public float[] GetQueryVector(string query, out string[] queryTerms)
        {
            // Crear una lista con las palabras normalizadas de la query y el vector correspondiente a la query relacionado con la matriz de TD-IDF
            List<string> queryTermsList = Normalizer(query).Keys.ToList();
            float[] queryVector = new float[termsNormalized.Count];

            // Iterar por la lista para rellenar el vector de la query
            int originalCount = queryTermsList.Count;
            for (int i = 0; i < originalCount; i++)
            {
                if (termsNormalized.Contains(queryTermsList[i]))
                    queryVector[termsNormalized.IndexOf(queryTermsList[i])]++;
                else
                    queryTermsList.RemoveAt(i); // Eliminar las palabras que no aparecen en ningun documento
            }

            // Devolver el vector de la query y sus palabras normalizadas
            queryTerms = queryTermsList.ToArray();
>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
            return queryVector;
        }

        #endregion

<<<<<<< HEAD
        #region Sugerencia 

        // Obtener la sugerencia a traves de la distancia Levinshtein 
        private string Suggestion(string term)
        {
            // Variables para guardar la palabra con la menor distancia
            string best = "";
            int distance = int.MaxValue;

            // Comparar la distnacia que hay entre la palabra de la query por cada palabra del universo
            foreach (var word in termsFrequencyPerDocument.Keys)
            {
                int distanceTemp = LevinshteinDistance(term, word);

                if (distanceTemp <= 1)
                    break;

                if (distanceTemp < distance)
                {
                    best = word;
                    distance = distanceTemp;
                }
            }

            return best;
        }

        // Calcular la distancia de Levinshtein
        public static int LevinshteinDistance(string s1, string s2)
        {
            // Crear un array que guardara dinamicamente los valores de la distacia de cada palabra
            int[,] dp = new int[s1.Length + 1, s2.Length + 1];

            // Llenar la 1ra columna y la 1ra fila del array dp
            for (int i = 0; i <= s1.Length; dp[i, 0] = i++) ;
            for (int j = 0; j <= s2.Length; dp[0, j] = j++) ;

            return LevinshteinDistanceDP(s1, s2, s1.Length - 1, s2.Length - 1, dp);
        }

        // Calcular la distancia de Levinshtein dinamicamente
        private static int LevinshteinDistanceDP(string s1, string s2, int i, int j, int[,] dp)
        {
            if (i == 0)
                return j;

            if (j == 0)
                return i;

            // Calcular el costo
            int cost = 0;
            if (s1[i - 1] != s2[j - 1])
                cost = 1;

            // Borrar un caracter
            if (dp[i - 1, j] == 0) 
            {
                dp[i - 1, j] = LevinshteinDistanceDP(s1, s2, i - 1, j, dp);
            }

            // Insetar un caracter
            if (dp[i, j - 1] == 0)
            {
                dp[i, j - 1] = LevinshteinDistanceDP(s1, s2, i, j - 1, dp);
            }

            // Sustituir un caracter
            if (dp[i - 1, j - 1] == 0)
            {
                dp[i - 1, j - 1] = LevinshteinDistanceDP(s1, s2, i - 1, j - 1, dp);
            }

            int delection = dp[i - 1, j] + 1;
            int insertion = dp[i, j - 1] + 1;
            int substition = dp[i - 1, j - 1] + cost;

            return Math.Min(delection, Math.Min(insertion, substition));
        }
        #endregion
=======

>>>>>>> 244ec07cc3b173c9e78ff228fa8281a3ac3e73b5
    }
}
