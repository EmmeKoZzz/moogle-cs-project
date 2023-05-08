using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MoogleEngine;

public class Corpus
{
  public Corpus()
  {
    // tiempo de computo
    var time = new Stopwatch();
    time.Start();

    CorpusWords = new Dictionary<string, float>();
    Matrix = new Dictionary<string, Dictionary<string, float>>();
    VectorialModel();

    time.Stop();
    Console.Write("Corpus: ");
    Console.WriteLine(time.Elapsed.TotalMilliseconds / 1000);
  }


  public static string[] Files = Directory.GetFiles("../Content", "*.txt", SearchOption.AllDirectories);

  // Cuerpo total de palabras digerentes en el universo de documetnos
  public Dictionary<string, float> CorpusWords { get; }

  // Matriz Documento Termino
  public Dictionary<string, Dictionary<string, float>> Matrix { get; }


  private void VectorialModel()
  {
    var CorpusWordRepetitions = new Dictionary<string, int>(); // contidad de documentos en que aparece

    foreach (var filePath in Files)
    {
      var fileWords = new Dictionary<string, float>(); // las palabras y sus repeticiones en el texto
      var fileWordsRepetitions = new Dictionary<string, int>(); // las palabras y su TF

      var lineWords = ProcessText(filePath);

      foreach (var word in lineWords)
      {
        // si la palabra no esta en el diccionario se agrega
        if (!CorpusWordRepetitions.ContainsKey(word))
        {
          CorpusWordRepetitions.Add(word, 0);
          CorpusWords.Add(word, 0);
        }

        // en lo q recorro las palabras voy calculando el TF y el IDF de cada una
        if (!fileWords.ContainsKey(word))
        {
          fileWordsRepetitions.Add(word, 1);
          fileWords.Add(word, 1 / (float)lineWords.Length);

          CorpusWordRepetitions[word]++;
          CorpusWords[word] = (float)Math.Log(Files.Length / (float)CorpusWordRepetitions[word]);
        }
        else
        {
          fileWordsRepetitions[word]++;
          fileWords[word] = fileWordsRepetitions[word] / (float)lineWords.Length;
        }
      }

      Matrix.Add(filePath, fileWords);
    }

    // Se calcula el TF*IDF aqui porque para tener el IDF debo haber pasado por todos los documentos
    foreach (var Dictionary in Matrix.Values)
    {
      foreach (var word in Dictionary.Keys) Dictionary[word] *= CorpusWords[word];
    }
  }


  // Procesar las palabras de un texto
  public static string[] ProcessText(string filePath)
  {
    return Regex.Split(NormaliceText(File.ReadAllText(filePath)), @"[\W ]+");
  }

  public static string NormaliceText(string text)
  {
    return text.ToLower()
      .Replace("á", "a")
      .Replace("é", "e")
      .Replace("í", "i")
      .Replace("ó", "o")
      .Replace("ú", "u")
      .Replace("ñ", "n");
  }
}
