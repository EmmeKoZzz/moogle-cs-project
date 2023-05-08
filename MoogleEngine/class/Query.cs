using System.Text.RegularExpressions;

namespace MoogleEngine;

internal class Query
{
  public Query(string query, Dictionary<string, float> corpusWords)
  {
    QueryWords = new List<string>();
    WordsNeeded = new List<string>();
    WordsUndesire = new List<string>();
    CloseWords = new List<string[]>();

    Vector = queryVector(query, corpusWords);

    if (QueryWords.Count != 0)
    {
      WordsNeeded = OperatorOfNeed(query);
      WordsUndesire = OperatorOfNeed(query, false);
      CloseWords = OperatorOfCloseness(query);
    }
  }

  //palabras clave para los operadores
  public List<string> WordsNeeded { get; }
  public List<string> WordsUndesire { get; }
  public List<string[]> CloseWords { get; }


  public Dictionary<string, float> Vector { get; } //representacion vectorial de la consulta
  private readonly List<string> QueryWords; //palabras de la query sin los operadores


  //compute el tF-idf de la query
  private Dictionary<string, float> queryVector(string query, Dictionary<string, float> corpusWords)
  {
    var vector = new Dictionary<string, float>();

    var Terms = Corpus.NormaliceText(query).Split(new[] { ' ', '~' }, StringSplitOptions.RemoveEmptyEntries);

    foreach (var TermWord in Terms)
    {
      //quitarle los operadores a las palabras
      var Word = Regex.Replace(TermWord, @"^\*+\^|^\*+|^!|^\^", "");
      QueryWords.Add(Word);

      // si la palabra existe en el cuerpo de palabras
      if (corpusWords.ContainsKey(Word))
      {
        if (corpusWords[Word] == 0) continue; // y su IDF es 0 entonces esa palabra es irrelevante

        if (!vector.ContainsKey(Word)) vector.Add(Word, 1);
        else vector[Word]++;
      }
    }

    if (vector.Count() == 0)
      return new Dictionary<string, float>(); // si no existe ninguna palabra relevante pues devuelve un vector nulo.

    //Computar el TF-IFD de cada palabra del vector si aparece en el diccionario de palabras
    foreach (var word in vector.Keys) vector[word] = vector[word] / Terms.Length * corpusWords[word];

    return computeImportant(vector, query);
  }


  //computa los operadores
  private Dictionary<string, float> computeImportant(Dictionary<string, float> vector, string query)
  {
    //computa el operador * de imoportancia
    var terms = query.Split(" ", StringSplitOptions.RemoveEmptyEntries);

    var maxWeidth = vector.Values.Max(); // mayor peso de la consulta

    foreach (var termRaw in terms)
    {
      var term = Regex.Replace(termRaw, @"^\*+\^|^\*+|^!|^\^|~+",
        ""); //quitar los operadores q puedan ir delante del de importancia

      if (term == "" || term[0] != '*') // si no existe operador de importancia continuar
        continue;
      foreach (var Char in term)
      {
        // por cada caracter de operador delante de la palabra se le aumneta el peso de la palabra de la consulta
        // con mayor relevancia tantas veces como operadores de imporatncia haya.
        if (Char == '*')
        {
          vector[term] += maxWeidth;

          continue;
        }

        break;
      }
    }

    return vector;
  }


  private List<string> OperatorOfNeed(string query, bool Need = true)
  {
    // palabras referidas a los operdores de necesidad
    var ListOfWords = new List<string>();

    var queryTerms = query.ToLower().Replace("~", " ~ ").Split(" ", StringSplitOptions.RemoveEmptyEntries);

    var i = 0;
    foreach (var word in queryTerms)
    {
      if (!Regex.Match(word, @"[a-z]").Success) continue;
      // palabra sin el posible signo de operador
      var Word = Regex.Replace(word, @"^\*+", "");

      if (Need && Word[0] == '^') // Lista de las palabras deseadas
        ListOfWords.Add(Regex.Replace(Word, @"^\^", ""));

      else if (!Need && Word[0] == '!') // Lista de las palabras no deseadas
        ListOfWords.Add(Regex.Replace(Word, @"^!", ""));

      i++;
    }

    return ListOfWords;
  }


  private List<string[]> OperatorOfCloseness(string query)
  {
    //Pareja de palabras referidas al operador de cercania
    // la lista a regresar
    var ParOfWords = new List<string[]>();

    var GroupOfWords = query.Split("~", StringSplitOptions.RemoveEmptyEntries);

    var len = 0;

    for (var i = 0; i < GroupOfWords.Length - 1; i++)
    {
      var words = GroupOfWords[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
      len += words.Length;

      string[] ClosesWords = { QueryWords[len].Trim(), QueryWords[len - 1].Trim() };

      //antes de agregar la pareja de palabras quiero acegurarme q no exista la misma pareja en la lista
      if (NotRepeat(ParOfWords, ClosesWords))
        ParOfWords.Add(ClosesWords);
    }

    return ParOfWords;
  }


  private static bool NotRepeat(List<string[]> ParOfWords, string[] CloseWords)
  {
    // encontrar las parejas de cercania q ya estan en la lista sin
    //importar el orden

    foreach (var Par in ParOfWords)
      if ((Par[0] == CloseWords[0] && Par[1] == CloseWords[1]) ||
          (Par[0] == CloseWords[1] && Par[1] == CloseWords[0]))
        return false;
    return true;
  }
}
