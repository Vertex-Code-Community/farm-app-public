using System.Text;

namespace FarmApp.Shared.Helpers;

public static class DebugHelper
{
    public static string SingleLineJsonToMultiline(string json)
    {
        int indentLevel = 0;
        var formattedJson = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '"';

        for (int i = 0; i < json.Length; i++)
        {
            char ch = json[i];

            // Toggle state of inQuotes
            if (ch == '"' || ch == '\'')
            {
                if (i > 0 && json[i - 1] != '\\')
                {
                    inQuotes = !inQuotes;
                    quoteChar = ch;
                }
            }

            // Handle characters based on whether we are inside quotes
            if (!inQuotes)
            {
                switch (ch)
                {
                    case '{':
                    case '[':
                        formattedJson.Append(ch);
                        formattedJson.Append("\n");
                        IncreaseIndent(ref indentLevel);
                        formattedJson.Append(new string('\t', indentLevel));
                        break;
                    case '}':
                    case ']':
                        formattedJson.Append("\n");
                        DecreaseIndent(ref indentLevel);
                        formattedJson.Append(new string('\t', indentLevel));
                        formattedJson.Append(ch);
                        break;
                    case ',':
                        formattedJson.Append(ch);
                        formattedJson.Append("\n");
                        formattedJson.Append(new string('\t', indentLevel));
                        break;
                    default:
                        formattedJson.Append(ch);
                        break;
                }
            }
            else
            {
                formattedJson.Append(ch);
            }
        }

        return formattedJson.ToString();
    }

    private static void IncreaseIndent(ref int indentLevel)
    {
        indentLevel++;
    }

    private static void DecreaseIndent(ref int indentLevel)
    {
        indentLevel--;
    }
}