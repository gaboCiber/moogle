# Moogle!

En el presente informe se detallará la realización del “Moogle”, una aplicación web de búsqueda matricial.

Esta aplicación buscará palabras o frases dentro de un contenedor que tiene una serie de documentos con extensión txt, y como resultado devolverá los documentos contengas esas palabras o frases. Para la optimización de la búsqueda se nos sugiere una serie de funcionalidades como por ejemplos: la utilización de modelos vectoriales, la introducción de operadores de consulta, etc.

Con este objetivo en mente, mi aplicación procesará los documentos antes de iniciar, es decir, que se calculará previamente una serie de valores necesarios e imprescindibles para la búsqueda. De esta tarea se encarga la clase Terms. Una clase que obtendrá una lista general de todas las palabras de los documentos, la cual se transformará en una matriz de TF-IDF (Term Frequency – Inverse Document Frequency).

Para la búsqueda realizada por un usuario (query) se dispone de una clase TextProcessor, la cual calculara un score por cada documento, donde ese score es la similitud que posee la query con un documento determinado. Y al final se devolverá como resultado de la búsqueda los documentos mas relevantes para la query.

