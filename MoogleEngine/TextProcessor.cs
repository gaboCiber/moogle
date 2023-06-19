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
        List<(char operador, int termIndex)> queryOperator;
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
        public SearchItem[] VectorialModel(string query, out string suggestion)
        {
            // Se obtienen el vector de la quers, sus palabras normalizadas
            float[] queryVector = terms.GetQueryVector(query, out queryTerms, out queryOperator, out suggestion);

            // Crear los array con los terminos de los operadores
            string[] excludedTerms;
            string[] includedTerms;
            string[] plusTerms;
            string[] importantTerms;
            (string, string)[] closerTerms;
            GetOperatorTerms(out excludedTerms, out includedTerms, out plusTerms, out importantTerms, out closerTerms);

            // Aumetar el tf de la vector de la query de las palabras del operador de importancia '*'
            PlusTheVector(plusTerms, queryVector);

            // // Aumetar el tf de la vector de la query de las palabras del operador de cercania '*'
            if (closerTerms.Length != 0)
            {
                string[] closerTermsUnified = new string[closerTerms.Length * 2];

                for (int i = 0, j = 0; i < closerTerms.Length; i++)
                {
                    closerTermsUnified[j++] = closerTerms[i].Item1;
                    closerTermsUnified[j++] = closerTerms[i].Item2;
                }

                PlusTheVector(closerTermsUnified, queryVector, 5);

            }
          
            // Se calcula el score a traves de la similitud de coseno y se obtiene el resultado de la busqueda
            float[] cosineSimilarity = CosineSimilarity(tfidfMatrix, queryVector);
            TitleCoincidence(cosineSimilarity);
            SearchItem[] queryDocumentsResult = QueryDocumentsResult();

            // Se ordena en orden descente el resultado de la busqueda y se devuelve
            Sort(queryDocumentsResult);
            return queryDocumentsResult;

            // Metodo auxiliar que relaciona los documentos con el score y busca el snippet dentro del documento
            SearchItem[] QueryDocumentsResult()
            {
                // Crear un array temporal para los resultados de la busqueda
                SearchItem[] tempResult = new SearchItem[documents.Count];
             
                // Iterar por los documentos 
                int indexResult = 0, indexDocument = 0;
                foreach (var title in documents.Keys)
                {
                    
                    // Verificar los operadores y el score: no se devolveran los documentos con score 0
                    if (cosineSimilarity[indexDocument] != 0 && !CheckExclusion(title, excludedTerms) && CheckInclusion(title, includedTerms))
                    {
                        // Variable que aumentar el score de un documento
                        float plusScore;

                        // Obtener el snippet del documento
                        string snippet = GetSnippet(title, indexDocument, importantTerms, closerTerms, out plusScore);
                        
                        float score = cosineSimilarity[indexDocument] *= plusScore;
                        
                        // Guardar los resultados
                        tempResult[indexResult++] = new SearchItem(title, snippet, score);
                    }
                                            
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
            if (normaQuery != 0)
            {
                for (int i = 0; i < documents.Count; i++)
                {
                    vectorSimilitud[i] = vectorResultante[i] / (normaMatrix[i] * normaQuery);
                } 
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

        private string GetSnippet(string title, int indexDocument, string[] signifivativeTerms, (string, string)[] closerTerms, out float plusScore)
        {
            // Inicializar el pluScore 
            plusScore = 1;
            
            // Fijar el tamaño de caracteres que debe tener el snippet
            int snippetlength = 200;

            // Para las palabras relacionadas con el operador de cercania
            if (closerTerms.Length != 0)
            {
                (int difference, int start, int end) snippetCercania = new (int.MaxValue, 0, 0);
                
                // Calcular dentro del documento la menor distancia que haya entre dos palabras del operador cercania
                for (int i = 0; i < closerTerms.Length; i++)
                {
                    // Obterner las posciones de las palabras dentro del documento
                    int[] positions1 = terms.GetPositions(title, closerTerms[i].Item1);
                    int[] positions2 = terms.GetPositions(title, closerTerms[i].Item2);

                    // Iternar por las posibles distancias 
                    for (int index1 = 0, index2 = 0; (index1 < positions1.Length && index2 < positions2.Length);)
                    {
                        int start = Math.Min(positions1[index1], positions2[index2]);
                        int end = Math.Max(positions1[index1], positions2[index2]);
                        int difference = end - start;

                        if (difference < snippetlength && difference < snippetCercania.difference)
                            snippetCercania = (difference, start, end);

                        if (positions1[index1] < positions2[index2])
                            index1++;
                        else
                            index2++;

                    }

                }

                if (snippetCercania != (int.MaxValue, 0, 0))
                {
                    plusScore = snippetlength / snippetCercania.difference;
                    return GetSnippetFromDocument(documents[title], snippetCercania.start, snippetCercania.end);
                }
            }

            //----------------------------------------------------------------------------------------------------------//

            /* Diccionario que trendra como
                - Key: las posiociones de un termino de la query
                - Value: el tfidf del respectivo termino  */
            Dictionary<int, float> queryPositions = new Dictionary<int, float>();

            /* Diccionario que tendra como
                - Key: el tfidf de los terminos de la query
                - Value: el numero de veces que aparece el termino en el snippet  */
            Dictionary<float, int> queryTFIDFValues = new Dictionary<float, int>();

            /* Lista que contiene las el tfidf de las palbras de los operadores '*' y '^' */
            List<float> relevantTermsList = new List<float>();

            // Iterar por los terminos de la query para rellenar los diccionario
            for (int i = 0, relevantIndex = 0; i < queryTerms.Length; i++)
            {
                // Descartar las palabras que no aparecen en la query
                if (terms.GetTermIndex(queryTerms[i]) == -1)
                    continue;

                // Obtener el TF-IDF del termino y agregarlo al diccionario
                float tfidfValue = tfidfMatrix[indexDocument, terms.GetTermIndex(queryTerms[i])];

                if (tfidfValue == 0)
                    continue;

                // Verificar si en el diccionario hay un tfidf igual al actual
                while (!queryTFIDFValues.TryAdd(tfidfValue, 0))
                    tfidfValue = MathF.BitDecrement(tfidfValue);
                
                //queryTFIDFValues.Add(tfidfValue, 0);

                // Verificar si el termino esta en significativeTerms
                if (relevantIndex < signifivativeTerms.Length && queryTerms[i] == signifivativeTerms[relevantIndex])
                {
                    relevantTermsList.Add(tfidfValue);
                    relevantIndex++;
                }

                // Obtener las posiciones de los terminos de la query en el documento y los agrega al diccionario  
                int[] positions = terms.GetPositions(title, queryTerms[i]);
                for (int j = 0; j < positions.Length; j++)
                {
                    queryPositions.Add(positions[j], tfidfValue);
                }
            }

            //----------------------------------------------------------------------------------------------------------//

            // Crear un array con las posiciones de los terminos ordenadas
            int[] queryPositionsSorted = (from position in queryPositions.Keys orderby position select position).ToArray();           

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
                    // Crear un score temporal y una variable que lleva el numero de terminos que hay dentro del snippet
                    float scoreTemp = 0;
                    int queryCount = 0;

                    // Boleano para identaficar cuando dentro del snippet aparece una palabra con operador
                    bool relavant = relevantTermsList.Count == 0;

                    // Iterar por el diccionario para calcular el score temporal
                    foreach (var key in queryTFIDFValues.Keys)
                    {
                        queryCount += ((queryTFIDFValues[key] > 0) ? 1 : 0);
                        scoreTemp += key * queryTFIDFValues[key];

                        if (queryTFIDFValues[key] > 0 && relevantTermsList.Contains(key))
                            relavant = true;

                        queryTFIDFValues[key] = 0;
                    }

                    scoreTemp *= MathF.Pow(queryCount, (relavant) ? 5 : 2);

                    // Comprobrar si el score temporal es mayor que el actual
                    if (scoreTemp > snippet.score)
                        snippet = (queryPositionsSorted[startIndex], queryPositionsSorted[finalIndex], scoreTemp);

                    startIndex = finalIndex + 1;
                }

            }

            return GetSnippetFromDocument(documents[title], snippet.start, snippet.end);

        }

        private string GetSnippetFromDocument(string text, int start, int end)
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

            //return BoldWords(snippet.ToString());
            return snippet.ToString();

        }

        #endregion

        #region Check Operators

        // Verificar y excluir de la busqueda si una palabra esta dentro de un documento (operador '!')
        private bool CheckExclusion(string title, string[] excludedTerms)
        {
            for (int i = 0; i < excludedTerms.Length; i++)
            {
                if (terms.GetFrequency(title, excludedTerms[i]) != 0)
                    return true;
            }

            return false;
        }

        // Verificar y asegurar en la busqueda si una palabra esta dentro de un documento (operador '^')
        private bool CheckInclusion(string title, string[] includedTerms)
        {
            for (int i = 0; i < includedTerms.Length; i++)
            {
                if (terms.GetFrequency(title, includedTerms[i]) == 0)
                    return false;
            }

            return true;
        }

        // Obtener las listas de los operadores con sus respectivas palabras
        private void GetOperatorTerms(out string[] excludedTerms, out string[] includedTerms, out string[] plusTerms, out string[] importantTerms, out (string, string)[] closerTerms)
        {
            List<string> excludedTermsList = new List<string>();
            List<string> includedTermsList = new List<string>();
            List<string> plusTemersList = new List<string>();
            List<string> importantTermsList = new List<string>();
            List<(string, string)> closerTermsList = new List<(string term1, string term2)>();
            

            for (int i = 0; i < queryOperator.Count; i++)
            {
                int index = (queryOperator[i].termIndex == queryTerms.Length) ? queryOperator[i].termIndex - 1 : queryOperator[i].termIndex;
                string word = queryTerms[index];

                switch (queryOperator[i].operador)
                {
                    case '!':
                        excludedTermsList.Add(word);
                        break;
                    case '^':
                        includedTermsList.Add(word);
                        break;
                    case '*':
                        plusTemersList.Add(word);
                        break;
                    case '~':
                        closerTermsList.Add((queryTerms[index - 1], word));
                        break;
                }

                if ((queryOperator[i].operador == '^' || queryOperator[i].operador == '*') && !importantTermsList.Contains(word))
                    importantTermsList.Add(word);
                    
            }

            excludedTerms = excludedTermsList.ToArray();
            includedTerms = includedTermsList.ToArray();
            plusTerms = plusTemersList.ToArray();
            importantTerms = importantTermsList.ToArray();
            closerTerms = closerTermsList.ToArray();

        }

        // Aumentar el tf de la query
        private void PlusTheVector(string[] plusTerms, float[] queryVector, int plusNumber = 1)
        {
            foreach (string word in plusTerms)
            {
                queryVector[terms.GetTermIndex(word)]+= plusNumber;
            }
        }

        #endregion

        #region Titles Coincidence

        public void TitleCoincidence(float[] cosineSimilarity)
        {
            int index = 0;
            foreach (var title in documents.Keys)
            {
                cosineSimilarity[index++] *= Coincidence(title);
            }

        }

        public float Coincidence(string title)
        {

            bool[] queryTermsMask = new bool[queryTerms.Length];

            StringBuilder wordSB = new StringBuilder();

            for (int i = 0; i < title.Length; i++)
            {
                if (!char.IsLetterOrDigit(title[i]))
                {
                    string word = wordSB.ToString();
                    wordSB.Clear();

                    if (string.IsNullOrWhiteSpace(word))
                        continue;

                    for (int j = 0; j < queryTerms.Length; j++)
                    {
                        if(word == queryTerms[j])
                            queryTermsMask[j] = true;
                    }
                }
                else
                {
                    char caracter = title[i];

                    switch (title[i])
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
                        default:
                            caracter = Convert.ToChar(title[i].ToString().ToLower());
                            break;
                    }

                    wordSB.Append(caracter);
                }
            }

            int numOfCoincidence = 0;
            for (int i = 0; i < queryTermsMask.Length; i++)
                numOfCoincidence += queryTermsMask[i] ? 1 : 0;

            float percent = numOfCoincidence / queryTerms.Length;
            return (percent >= 0.60) ? percent * 5 : 1;
        }

        #endregion
    }
}

