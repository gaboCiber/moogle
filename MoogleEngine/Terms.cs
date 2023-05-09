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

        // Lista que guardara las palabras normalizados de todos los documentos (sin repetir)
        List<string> termsNormalized;

        /* Diccionario que tiene como
           - Key: los nombres de los documentos .txt
           - Value: el texto de cada documentos .txt  */
        Dictionary<string, string> documents;

        /* Diccionario que tiene como
           - Key: las palabras normalizados de todos los documentos (sin repetir)
           - Value: el numero de documentos donde aparece la palabra  */
        Dictionary<string, int> termsFrequencyPerDocument;

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
            termsNormalized = new List<string>();
            termsLineAndFrequency = new Dictionary<string, Dictionary<string, List<int>>>(); // Position 0: line | Position 1: frequency
            termsFrequencyPerDocument = new Dictionary<string, int>();
            maxTermsFrequencyOfEachDocument = new Dictionary<string, int>();

            // Get their keys and Values
            NormalizeAndFrequency();

            // Calculate the TF-IDF Matrix
            tfidfMatrix = new float[documents.Count, termsNormalized.Count];
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

            // Obtener las palabras normalizadas
            termsNormalized = termsFrequencyPerDocument.Keys.ToList();
        }

        // Metodo que normaliza y obtiene las diferentes frecuencias de un documento
        private Dictionary<string, List<int>> Normalizer(string text)
        {
            // Crear el diccionario que sera devuelto por el metodo
            Dictionary<string, List<int>> terms = new Dictionary<string, List<int>>();
            
            // Variable que guardara una palabra normalizada a medida que se procesa el texto
            StringBuilder actualWord = new StringBuilder();

            // Iterar sobre la variable text y determinar si un caracter es una letra o un numero
            int i;
            for (i = 0; i < text.Length; i++)
            {
                // 
                if (!char.IsLetterOrDigit(text[i]))
                {
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

                    }

                    terms[word].Add(i);

                    // Para maxTermsFrequencyOfEachDocument
                    if (maxTermsFrequencyOfEachDocument.Values.Last() < terms[word].Count)
                        maxTermsFrequencyOfEachDocument[maxTermsFrequencyOfEachDocument.Keys.Last()] = terms[word].Count;

                    actualWord.Clear();
                }
            }

        }

        #endregion

        #region Get Values

        // Devuelve el indice de una palabra que se encuentra en la lista termsNormalized
        public int GetTermIndex(string term)
        {
            return termsNormalized.IndexOf(term);
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
                foreach (var term in termsNormalized)
                {
                    float num = CalculateTF(GetFrequency(title, term), maxTermsFrequencyOfEachDocument[title]) * CalculateIDF(documents.Count, termsFrequencyPerDocument[term]);
                    
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
            return queryVector;
        }

        #endregion


    }
}
