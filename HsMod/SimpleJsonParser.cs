using System;
using System.Collections.Generic;
using System.Text;

namespace HsMod
{
    /// <summary>
    /// Simple JSON parser for Dictionary<string, string> to replace Newtonsoft.Json dependency
    /// This avoids TypeLoadException issues with Newtonsoft.Json.Linq.JObject in BepInEx environment
    /// </summary>
    public static class SimpleJsonParser
    {
        /// <summary>
        /// Deserializes a simple JSON object into a Dictionary<string, string>
        /// Handles basic JSON key-value pairs only
        /// </summary>
        public static Dictionary<string, string> ParseJsonObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var result = new Dictionary<string, string>();
            json = json.Trim();

            if (!json.StartsWith("{") || !json.EndsWith("}"))
                return null;

            // Remove outer braces
            json = json.Substring(1, json.Length - 2).Trim();

            if (string.IsNullOrEmpty(json))
                return result;

            int i = 0;
            while (i < json.Length)
            {
                // Skip whitespace and commas
                while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ','))
                    i++;

                if (i >= json.Length)
                    break;

                // Parse key
                string key = ParseString(json, ref i);
                if (key == null)
                    break;

                // Skip whitespace and colon
                while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ':'))
                    i++;

                if (i >= json.Length)
                    break;

                // Parse value
                string value = ParseValue(json, ref i);
                if (value != null)
                {
                    result[key] = value;
                }
            }

            return result;
        }

        private static string ParseString(string json, ref int index)
        {
            // Skip whitespace
            while (index < json.Length && char.IsWhiteSpace(json[index]))
                index++;

            if (index >= json.Length || json[index] != '"')
                return null;

            index++; // Skip opening quote
            var sb = new StringBuilder();
            bool escaped = false;

            while (index < json.Length)
            {
                char c = json[index];

                if (escaped)
                {
                    // Handle escape sequences
                    switch (c)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            sb.Append(c);
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'u':
                            // Unicode escape sequence \uXXXX
                            if (index + 4 < json.Length)
                            {
                                string hex = json.Substring(index + 1, 4);
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int unicodeValue))
                                {
                                    sb.Append((char)unicodeValue);
                                    index += 4;
                                }
                            }
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    index++; // Skip closing quote
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }

                index++;
            }

            return null; // Unterminated string
        }

        private static string ParseValue(string json, ref int index)
        {
            // Skip whitespace
            while (index < json.Length && char.IsWhiteSpace(json[index]))
                index++;

            if (index >= json.Length)
                return null;

            char c = json[index];

            // String value
            if (c == '"')
            {
                return ParseString(json, ref index);
            }

            // Number, boolean, or null
            var sb = new StringBuilder();
            while (index < json.Length)
            {
                c = json[index];
                if (c == ',' || c == '}' || c == ']' || char.IsWhiteSpace(c))
                    break;

                sb.Append(c);
                index++;
            }

            return sb.ToString().Trim();
        }
    }
}
