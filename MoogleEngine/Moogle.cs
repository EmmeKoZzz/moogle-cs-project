using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MoogleEngine;

public partial class Moogle
{
	public static Corpus? DocCorpus;

	public static SearchResult Query(
	  string query)
	{
		// tiempo de computo
		var time = new Stopwatch();
		time.Start();

		// si no existe ninguna palabra en la query se invalida
		if (!Regex.Match(query, @"\w").Success)
		{
			SearchItem[] Excep = { new("Por favor, haga una consulta valida", "gracias", 0) };
			return new SearchResult(Excep);
		}

		var Matrix = DocCorpus!.Matrix;
		var corpusWords = DocCorpus.CorpusWords;


		var Query = new Query(query, corpusWords); // Consulta procesada

		if (Query.Vector.Count == 0) // si la consulta es irrelevante sugerir una q si devuelva un resultado
			return new SearchResult(new SearchItem[]{new SearchItem("no se encontraron documentos afines a su consulta.", "", 0)}, ComputeSuggestion(query, corpusWords, true));

		var itemss = ListDocs(query, Query, Matrix);

		var Suggestion = "";
		if (itemss.Length < 5)
			Suggestion = ComputeSuggestion(query, corpusWords);

		time.Stop();
		Console.Write("Moogle: ");
		Console.WriteLine(time.Elapsed.TotalMilliseconds / 1000);

		return new SearchResult(itemss, Suggestion);
	}


	//Crear la Lista de Documentos Relevantes
	private static SearchItem[] ListDocs(string queryString, Query query,
	  Dictionary<string, Dictionary<string, float>> Matrix)
	{
		var Docs = new List<SearchItem>();
		var TempDocs = new Dictionary<string, float>();

		foreach (var document in Matrix)
		{
			//si los operadores no aprueban el documento q lo salten
			if (!OperatorOfNeed(query.WordsNeeded, document.Value) ||
				 !OperatorOfVanish(query.WordsUndesire, document.Value)) continue;

			// si el documento no contiene ningun palabra de la query no se muestra
			var Bypass = false;

			foreach (var word in query.Vector.Keys)
				if (document.Value.ContainsKey(word))
					Bypass = true;

			if (Bypass == false) continue;

			// Computa el Score del documento
			var DocOnWords = Corpus.ProcessText(document.Key);
			var Score = ComputeScore(query.CloseWords, query.Vector, DocOnWords.ToArray(), document.Value);

			TempDocs.Add(document.Key, Score);
		}

		var AmountDocs = 0;
		foreach (var Document in TempDocs.OrderByDescending(Key => Key.Value))
		{
			if (AmountDocs >= 10) break;

			var Title = Document.Key.Split(@"\").Last().Replace(".txt", "");
			var Snippet = ComputeSnippet(Document.Key, query.Vector);

			if (Snippet == "")
				continue; // si aquesta funcion no encuentra la palabra pos entonces la palabra no existe en en doc

			Docs.Add(new SearchItem(Title, Snippet, Document.Value));
			AmountDocs++;
		}

		return Docs.ToArray();
	}

	//Calcular la similitud de los documetos con la query con el metodo de similitud del Coseno
	private static float ComputeScore(List<string[]> CloseWords, Dictionary<string, float> QueryVector,
	  string[] DocOnWords, Dictionary<string, float> DocVector)
	{
		var CosineScore = DCosine(QueryVector, DocVector); // calcula la similitud por el coseno de los vectores

		if (CloseWords.Count == 0) return CosineScore;

		//a la similitud entre vectores le sumo el porcentaje de cercania de lo q le falta para uno q es el maximo score
		return CosineScore + (1 - CosineScore) * ClosenessOperator(CloseWords, DocOnWords, DocVector);
	}

	private static float DCosine(Dictionary<string, float> queryVector, Dictionary<string, float> docVector)
	{
		// calcula la similitud entre dos vectores a traves de q tan pequenno sea el angulo entre los dos vectores
		float Numerator = 0, DenQuery = 0, DenDoc = 0;

		foreach (var Word in queryVector.Keys)
		{
			DenQuery += queryVector[Word] * queryVector[Word];

			if (!docVector.ContainsKey(Word)) continue;
			Numerator += queryVector[Word] * docVector[Word];
		}

		foreach (var Weight in docVector.Values)
			DenDoc += Weight * Weight;

		return (float)(Numerator / (Math.Sqrt(DenQuery) * Math.Sqrt(DenDoc)));
	}


	//Computar el fragmento de texto q sea mas denso
	private static string ComputeSnippet(string DocPath, Dictionary<string, float> queryVector)
	{
		//la lista d las palabras del documento sin procesasr los caracteres especiales
		var TrueDocWords = Regex.Matches(File.ReadAllText(DocPath), @"\S+");

		//la lista de las posiciones de las palabras de la consulta en el documento
		var PositionsOfQueryWords = PositionsOf(queryVector, TrueDocWords);

		if (PositionsOfQueryWords.Count == 0) return ""; // sino encuentra la palabra entonces no hay nada mas q hacer aqui

		int PositionOfSnip = 0, DensityMax = 0, DensityTemp, k;
		if (PositionsOfQueryWords.Count == 1) k = 0;
		else k = 1;

		for (var i = 0; i < PositionsOfQueryWords.Count - k; i++)
		{
			DensityTemp = 1;
			for (var j = i + k; j < PositionsOfQueryWords.Count; j++)
				if (PositionsOfQueryWords[j] < PositionsOfQueryWords[i] + 80) DensityTemp++;
				else break;


			if (DensityTemp > DensityMax)
			{
				DensityMax = DensityTemp;

				if (PositionsOfQueryWords[i] - 10 <= 0) PositionOfSnip = 0;
				else
					PositionOfSnip = i;
			}
		}

		var Snippet = "";
		int Length = 100, StartWord = 0;

		if (TrueDocWords.Count < 100)
			Length = TrueDocWords.Count;
		else if (PositionsOfQueryWords[PositionOfSnip] + 100 > TrueDocWords.Count)
			StartWord = TrueDocWords.Count - 100;
		else if (PositionsOfQueryWords[PositionOfSnip] - 10 >= 0)
			StartWord = PositionsOfQueryWords[PositionOfSnip] - 10;
		else StartWord = 0;


		for (var i = 0; i < Length; i++)
			Snippet += TrueDocWords[StartWord + i].Value + " ";

		return Snippet;
	}

	private static List<int> PositionsOf(Dictionary<string, float> KeyWords, MatchCollection TextToSeach)
	{
		List<int> Positions = new();

		var Position = 0;
		string Word;
		foreach (Match WordMatch in TextToSeach)
		{
			Word = Corpus.NormaliceText(WordMatch.Value);
			foreach (var KeyWord in KeyWords)
				if ((KeyWord.Value != 0 || Corpus.Files.Length == 1) && Word.Contains(KeyWord.Key))
					if (Regex.Match(KeyWord.Key, @"\W").Success || Word == KeyWord.Key)
						Positions.Add(Position);

			Position++;
		}

		return Positions;
	}


	// Computar la sugerencia
	private static string ComputeSuggestion(string query, Dictionary<string, float> corpusWords, bool on = false)
	{
		var queryWords = MyRegex().Replace(Corpus.NormaliceText(query), " ~ ")
		  .Split(' ', StringSplitOptions.RemoveEmptyEntries);

		var IdfMin = Corpus.Files.Length < 5 ? int.MaxValue :
		  (float)Math.Log10(Corpus.Files.Length /
								  (float)5); // IDF de una palabra q aparece en la contidad maxima de docuemtnos para mostrar la sugerencia

		// averiguar si tiene operadores
		string Suggestion = "", SuggestionWord = "";

		foreach (var RawTerm in queryWords)
		{
			//obtengo toda la informacion relacionada de los operadores
			var Operator = MyRegex2().Match(RawTerm);

			var Word = RawTerm;

			if (Operator.Success)
			{
				// en el termino se elimina cualquier operador
				Word = MyRegex1().Replace(Word, "");

				if (Word == "")
				{
					if (RawTerm == "~" && // si el termino es el operacdor de cercania
						 Suggestion.Length > 2 && // si no nos encontramos al inicio de la sugerencia
						 Suggestion[^2] != '~' // y si los ultimos caracteres escrito no son "~ "
						)
						Suggestion += "~ "; // entonces escribir en la sugerencia el operador de cercania

					continue; // si el termino esa un operador salta al siguiente ciclo
				}
			}

			if (corpusWords.ContainsKey(Word))
			{
				if (corpusWords[Word] == 0 && !on)
				{
					// si el IDF de la palabra es 0 o aprece en 6 o mas docs
					Suggestion +=
					  $"{Operator.Value}{Word} "; // se agrega sin mas la palabra a la sugerencia con su respectivo operador
					continue; // si la palabra es conocida e irrelevante pasar al siguiente ciclo
				}

				if (corpusWords[Word] > IdfMin || on) // si existe pero en menos de 6 documentos sugerir una palabra mas comun
					SuggestionWord = SuggestWord(Word, corpusWords, IdfMin);
			}
			else
			{
				// y, bueno, si no existe sugiere una q si exista.... no?
				SuggestionWord = SuggestWord(Word, corpusWords, IdfMin);
			}

			Suggestion += $"{SuggestionWord} ";
		}

		return Regex.Replace(Suggestion.TrimEnd(), "~+$", "");
	}

	private static string SuggestWord(string Word, Dictionary<string, float> corpusWords, float MinIdf)
	{
		// Esta Funciion me devuelve una palabra mas ... "adecuada"?
		var MinDistance = int.MaxValue;
		var suggestedWord = "";

		foreach (var Target in corpusWords.Keys) // verificar cada palabra del universo de palabras
		{
			if (corpusWords[Target] > MinIdf || corpusWords[Target] == 0 || Target == Word) continue;
			// si la palabra q se esta verificando es la misma q se intenta cambiar
			// o es irrelevante o aparece en menos de 6 documentos entonces salta a la iguiente palabra del universo

			var DistanceTemp = LevenshteinDistance(Word,
			  Target);
			//... si es menor q la distancia minima q se tiene registrada se remplaza, al final se devuelve la palabra q obtuvo la distancia minoma

			if (DistanceTemp < MinDistance)
			{
				MinDistance = DistanceTemp;
				suggestedWord = Target;
			}
		}

		return suggestedWord;
	}

	private static int LevenshteinDistance(string word, string target)
	{
		if (word == target) // i la palabra es la misma la distancia es 0
			return 0;

		if (word.Length == 0) return target.Length;

		if (target.Length == 0) return word.Length;

		var vR = new int[target.Length + 1];
		var vC = new int[target.Length + 1];

		for (var i = 0; i < vR.Length; i++) vR[i] = i;

		for (var i = 0; i < word.Length; i++)
		{
			vC[0] = i + 1;

			for (var j = 0; j < target.Length; j++)
			{
				var cost = 1;

				if (word[i] == target[j]) cost = 0;

				vC[j + 1] = Math.Min(vC[j] + 1,
				  Math.Min(
					 vR[j + 1] + 1,
					 vR[j] + cost
				  )
				);
			}

			vR = vC;
			vC = new int[target.Length + 1];
		}

		return vR[target.Length];
	}


	// operadores
	private static bool OperatorOfVanish(List<string> WordsUndesire, Dictionary<string, float> WordsOfDoc)
	{
		foreach (var word in WordsUndesire)
			if (WordsOfDoc.ContainsKey(word))
				return false;
		return true;
	}

	private static bool OperatorOfNeed(List<string> WordsNeeded, Dictionary<string, float> WordsOfDoc)
	{
		foreach (var word in WordsNeeded)
			if (!WordsOfDoc.ContainsKey(word))
				return false;
		return true;
	}

	private static float ClosenessOperator(List<string[]> ParOfWords, string[] DocWords,
	  Dictionary<string, float> WordsOfDoc)
	{
		int WordsBetween = 0, MinDistance = DocWords.Length - 2;
		string TargetWord = "", CurrentWord = "";
		float ClosenessPorcent = 0;
		var check = false;

		foreach (var Words in ParOfWords) // por cada par de palabras
		{
			if (!WordsOfDoc.ContainsKey(Words[0]) || !WordsOfDoc.ContainsKey(Words[1]))
				continue; // si alguna de las dos no existe no proceses el par

			for (var i = 0; i < DocWords.Length; i++)
			{
				if (check == false)
				{
					if (DocWords[i] == Words[0])
					{
						CurrentWord = Words[0];
						TargetWord = Words[1];
						check = true;
					}
					else if (DocWords[i] == Words[1])
					{
						CurrentWord = Words[1];
						TargetWord = Words[0];
						check = true;
					}

					continue;
				}

				if (DocWords[i] == CurrentWord)
				{
					WordsBetween = 0;
				}
				else if (DocWords[i] == TargetWord)
				{
					if (WordsBetween < MinDistance) MinDistance = WordsBetween;

					WordsBetween = 0;

					(CurrentWord, TargetWord) = (TargetWord, CurrentWord);
				}
				else
				{
					WordsBetween++;
				}
			}

			//calcular el porcentaje (en base de 1, es decir el porunidaje xD) de cercania de la pareja en el documento
			ClosenessPorcent += 1 - 1 * (MinDistance / (float)(DocWords.Length - 2));
		}

		//Luego Calculo el promedio de los porunidajes de todas las parejas de palabras
		return ClosenessPorcent / ParOfWords.Count;
	}

	[GeneratedRegex("~+")]
	private static partial Regex MyRegex();

	[GeneratedRegex("^\\*+\\^|^\\*+|^!|^\\^|~+")]
	private static partial Regex MyRegex1();

	[GeneratedRegex("^\\*+\\^|^\\*+|^!|^\\^|~+")]
	private static partial Regex MyRegex2();
}
