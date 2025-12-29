using Article_Graph_Analysis_Application.Models;
using System.IO;
using System.Text;

namespace Article_Graph_Analysis_Application.Services.Json
{
	public class JsonPaperLoader
	{
		public static List<Paper> LoadPapers(string filePath)
		{
			var papers = new List<Paper>();

			try
			{
				string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
				papers = ParseJsonManually(jsonContent);
			}
			catch (Exception ex)
			{
				throw new Exception($"JSON yükleme hatası: {ex.Message}");
			}

			return papers;
		}

		private static List<Paper> ParseJsonManually(string json)
		{
			var papers = new List<Paper>();
			json = json.Trim();

			if (!json.StartsWith("[") || !json.EndsWith("]"))
			{
				throw new Exception("JSON array formatında değil");
			}

			json = json.Substring(1, json.Length - 2).Trim();

			int depth = 0;
			int start = 0;

			for (int i = 0; i < json.Length; i++)
			{
				if (json[i] == '{') depth++;
				if (json[i] == '}') depth--;

				if (depth == 0 && json[i] == '}')
				{
					string objectJson = json.Substring(start, i - start + 1).Trim();
					var paper = ParsePaperObject(objectJson);
					if (paper != null)
					{
						papers.Add(paper);
					}

					while (i + 1 < json.Length && (json[i + 1] == ',' || char.IsWhiteSpace(json[i + 1])))
					{
						i++;
					}
					start = i + 1;
				}
			}

			return papers;
		}

		private static Paper ParsePaperObject(string objectJson)
		{
			var paper = new Paper();

			objectJson = objectJson.Trim();
			if (objectJson.StartsWith("{")) objectJson = objectJson.Substring(1);
			if (objectJson.EndsWith("}")) objectJson = objectJson.Substring(0, objectJson.Length - 1);

			var fields = SplitJsonFields(objectJson);

			foreach (var field in fields)
			{
				var kvp = ParseKeyValue(field);
				if (kvp.HasValue)
				{
					var key = kvp.Value.Key;
					var value = kvp.Value.Value;

					switch (key)
					{
						case "id":
							paper.Id = value.Trim('"');
							break;
						case "title":
							paper.Title = value.Trim('"');
							break;
						case "year":
							if (int.TryParse(value, out int year))
								paper.Year = year;
							break;
						case "authors":
							paper.Authors = ParseStringArray(value);
							break;
						case "referenced_works":
							paper.ReferencedWorks = ParseStringArray(value);
							break;
						case "in_json_reference_count":  // ← EKLE
							if (int.TryParse(value, out int citCount))
								paper.InCitationCount = citCount;
							break;
					}
				}
			}

			return paper;
		}

		private static List<string> SplitJsonFields(string json)
		{
			var fields = new List<string>();
			int depth = 0;
			int bracketDepth = 0;
			int start = 0;
			bool inString = false;
			char prevChar = ' ';

			for (int i = 0; i < json.Length; i++)
			{
				char c = json[i];

				if (c == '"' && prevChar != '\\')
				{
					inString = !inString;
				}

				if (!inString)
				{
					if (c == '{') depth++;
					if (c == '}') depth--;
					if (c == '[') bracketDepth++;
					if (c == ']') bracketDepth--;

					if (c == ',' && depth == 0 && bracketDepth == 0)
					{
						fields.Add(json.Substring(start, i - start).Trim());
						start = i + 1;
					}
				}

				prevChar = c;
			}

			if (start < json.Length)
			{
				fields.Add(json.Substring(start).Trim());
			}

			return fields;
		}

		private static KeyValuePair<string, string>? ParseKeyValue(string field)
		{
			int colonIndex = field.IndexOf(':');
			if (colonIndex == -1) return null;

			string key = field.Substring(0, colonIndex).Trim().Trim('"');
			string value = field.Substring(colonIndex + 1).Trim();

			return new KeyValuePair<string, string>(key, value);
		}

		private static List<string> ParseStringArray(string arrayJson)
		{
			var result = new List<string>();
			arrayJson = arrayJson.Trim();

			if (!arrayJson.StartsWith("[") || !arrayJson.EndsWith("]"))
			{
				return result;
			}

			arrayJson = arrayJson.Substring(1, arrayJson.Length - 2).Trim();

			if (string.IsNullOrEmpty(arrayJson))
			{
				return result;
			}

			bool inString = false;
			int start = 0;
			char prevChar = ' ';

			for (int i = 0; i < arrayJson.Length; i++)
			{
				char c = arrayJson[i];

				if (c == '"' && prevChar != '\\')
				{
					inString = !inString;
				}

				if (!inString && c == ',')
				{
					string item = arrayJson.Substring(start, i - start).Trim().Trim('"');
					if (!string.IsNullOrEmpty(item))
					{
						result.Add(item);
					}
					start = i + 1;
				}

				prevChar = c;
			}

			if (start < arrayJson.Length)
			{
				string item = arrayJson.Substring(start).Trim().Trim('"');
				if (!string.IsNullOrEmpty(item))
				{
					result.Add(item);
				}
			}

			return result;
		}
	}
}