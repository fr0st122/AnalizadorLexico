using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalizadorLexico_V2
{
    // Módulo léxico: convierte texto de entrada en tokens y detecta el modo código/texto.
    public enum InputMode
    {
        Auto,
        Codigo,
        Texto
    }
    public enum Language
    {
        Kotlin,
        CSharp,
        Java
    }

    public class Lexer
    {
        private HashSet<string> _keywords = new(StringComparer.OrdinalIgnoreCase);
        public Language SelectedLanguage { get; private set; }

        public Lexer() : this(Language.CSharp)
        {
        }

        public Lexer(Language language)
        {
            SetLanguage(language);
        }

        public void SetLanguage(Language language)
        {
            SelectedLanguage = language;

            switch (language)
            {
                case Language.Kotlin:
                    _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "var","val","fun","if","else","when","while","for","return","true","false","class","object","interface","package","import","override","in","is","as"
                    };
                    break;
                case Language.CSharp:
                    _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "var","if","else","while","for","return","true","false","class","namespace","using","public","private","protected","static","void","new","interface","enum","struct",
                        "int","string","bool","double","float","decimal","long","short","byte","char"
                    };
                    break;
                case Language.Java:
                    _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "if","else","while","for","return","true","false","class","package","import","public","private","protected","static","void","new","interface","extends","implements","try","catch",
                        "int","String","boolean","double","float","long","short","byte","char"
                    };
                    break;
                default:
                    _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    break;
            }
        }

        private Language? LanguageFromExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            var ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
            return ext switch
            {
                ".kt" => Language.Kotlin,
                ".kts" => Language.Kotlin,
                ".java" => Language.Java,
                ".cs" => Language.CSharp,
                _ => null
            };
        }

        public List<TokenInfo> Analizar(string input)
        {
            return Analizar(input, InputMode.Auto);
        }

        public List<TokenInfo> Analizar(string input, Language language)
        {
            var prev = SelectedLanguage;
            try
            {
                SetLanguage(language);
                return Analizar(input, InputMode.Auto);
            }
            finally
            {
                SetLanguage(prev);
            }
        }

        public List<TokenInfo> Analizar(string input, string filePath)
        {
            var lang = LanguageFromExtension(filePath);
            if (lang.HasValue)
                return Analizar(input, lang.Value);

            return Analizar(input);
        }

        public List<TokenInfo> Analizar(string input, InputMode mode)
        {
            if (string.IsNullOrEmpty(input))
                return new List<TokenInfo>();

            if (mode == InputMode.Auto)
                mode = DetectMode(input);

            return mode == InputMode.Codigo ? AnalizarCodigo(input) : AnalizarTexto(input);
        }

        public List<Diagnostic> Diagnosticar(string input, string filePath)
        {
            var lang = LanguageFromExtension(filePath);
            if (lang.HasValue)
                return Diagnosticar(input, lang.Value);

            return Diagnosticar(input);
        }

        public List<Diagnostic> Diagnosticar(string input, Language language)
        {
            var diagnostics = new List<Diagnostic>();
            if (string.IsNullOrEmpty(input)) return diagnostics;

            if (!LanguageRules.SemicolonRequired(language))
                return diagnostics;

            var lines = input.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.EndsWith("{") || line.EndsWith("}") || line.EndsWith(")") || line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith("/*") || line.EndsWith("*/"))
                    continue;

                if (!line.EndsWith(";") && Regex.IsMatch(line, @"[A-Za-z0-9_].*"))
                {
                    var trimmedStart = line.TrimStart();
                    if (Regex.IsMatch(trimmedStart, "^(if|for|while|else|switch|try|catch|class|interface|enum|public|private|protected|return)\\b", RegexOptions.IgnoreCase))
                        continue;

                    diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Sintáctico", "Falta ';' al final de la sentencia (segun lenguaje seleccionado).", i + 1, Math.Max(1, line.Length)));
                }
            }

            return diagnostics;
        }

        public InputMode DetectarModo(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return InputMode.Texto;

            return DetectMode(input);
        }

        private InputMode DetectMode(string input)
        {
            int codeChars = 0;
            string codeSymbols = "{}[]();=+-*/<>!&|.:\"";
            foreach (char c in input)
            {
                if (codeSymbols.IndexOf(c) >= 0) codeChars++;
            }

            int keywordCount = 0;
            var detectedLang = DetectLanguageFromContent(input);
            var keywordSet = GetKeywordSet(detectedLang);
            foreach (var kw in keywordSet)
            {
                if (Regex.IsMatch(input, $@"\b{Regex.Escape(kw)}\b", RegexOptions.IgnoreCase))
                    keywordCount++;
            }

            double ratio = (double)codeChars / Math.Max(1, input.Length);
            if (ratio > 0.03 || keywordCount > 0)
                return InputMode.Codigo;

            return InputMode.Texto;
        }

        private List<TokenInfo> AnalizarCodigo(string input)
        {
            var tokens = new List<TokenInfo>();
            int posicion = 0;
            int linea = 1;
            int columna = 1;

            // Evita perder palabras reservadas cuando el modo se detecta antes del lenguaje exacto.
            var combinedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in GetKeywordSet(Language.Kotlin)) combinedKeywords.Add(k);
            foreach (var k in GetKeywordSet(Language.CSharp)) combinedKeywords.Add(k);
            foreach (var k in GetKeywordSet(Language.Java)) combinedKeywords.Add(k);

            while (posicion < input.Length)
            {
                char actual = input[posicion];

                if (actual == '\n')
                {
                    posicion++;
                    linea++;
                    columna = 1;
                    continue;
                }

                if (char.IsWhiteSpace(actual))
                {
                    posicion++;
                    columna++;
                    continue;
                }

                // Cadenas normales, verbatim de C# y raw strings de Kotlin.
                if (actual == '"' || (actual == '@' && posicion + 1 < input.Length && input[posicion + 1] == '"'))
                {
                    int start = posicion;
                    int startLine = linea;
                    int startCol = columna;
                    var sb = new StringBuilder();

                    bool isVerbatim = false;
                    if (actual == '@')
                    {
                        isVerbatim = true;
                        sb.Append('@');
                        posicion++;
                        columna++;
                        if (posicion >= input.Length || input[posicion] != '"')
                        {
                            tokens.Add(new TokenInfo(TokenType.ERROR, sb.ToString(), start, startLine, startCol));
                            continue;
                        }
                        actual = '"';
                    }

                    if (!isVerbatim && posicion + 2 < input.Length && input[posicion] == '"' && input[posicion + 1] == '"' && input[posicion + 2] == '"' && SelectedLanguage == Language.Kotlin)
                    {
                        sb.Append("\"\"\"");
                        posicion += 3;
                        columna += 3;
                        bool closed = false;
                        while (posicion < input.Length)
                        {
                            if (posicion + 2 < input.Length && input[posicion] == '"' && input[posicion + 1] == '"' && input[posicion + 2] == '"')
                            {
                                sb.Append("\"\"\"");
                                posicion += 3;
                                columna += 3;
                                closed = true;
                                break;
                            }

                            char c = input[posicion];
                            sb.Append(c);
                            if (c == '\n') { linea++; columna = 1; } else columna++;
                            posicion++;
                        }

                        tokens.Add(new TokenInfo(closed ? TokenType.STRING : TokenType.ERROR, sb.ToString(), start, startLine, startCol));
                        continue;
                    }

                    sb.Append('"');
                    posicion++;
                    columna++;

                    bool closedNormal = false;
                    int backslashes = 0;

                    if (isVerbatim)
                    {
                        while (posicion < input.Length)
                        {
                            char c = input[posicion];
                            if (c == '"')
                            {
                                if (posicion + 1 < input.Length && input[posicion + 1] == '"')
                                {
                                    sb.Append("\"\"");
                                    posicion += 2;
                                    columna += 2;
                                    continue;
                                }
                                sb.Append('"');
                                posicion++;
                                columna++;
                                closedNormal = true;
                                break;
                            }

                            sb.Append(c);
                            if (c == '\n') { linea++; columna = 1; } else columna++;
                            posicion++;
                        }

                        tokens.Add(new TokenInfo(closedNormal ? TokenType.STRING : TokenType.ERROR, sb.ToString(), start, startLine, startCol));
                        continue;
                    }

                    while (posicion < input.Length)
                    {
                        char c = input[posicion];
                        sb.Append(c);

                        if (c == '\n') { linea++; columna = 1; }
                        else { columna++; }

                        if (c == '\\')
                        {
                            backslashes++;
                        }
                        else if (c == '"')
                        {
                            if (backslashes % 2 == 0)
                            {
                                posicion++;
                                closedNormal = true;
                                break;
                            }
                            backslashes = 0;
                        }
                        else
                        {
                            backslashes = 0;
                        }

                        posicion++;
                    }

                    tokens.Add(new TokenInfo(closedNormal ? TokenType.STRING : TokenType.ERROR, sb.ToString(), start, startLine, startCol));
                    continue;
                }

                if (actual == '\'')
                {
                    int start = posicion;
                    int startLine = linea;
                    int startCol = columna;
                    var sb = new StringBuilder();
                    sb.Append('\'');
                    posicion++;
                    columna++;
                    bool closed = false;

                    while (posicion < input.Length)
                    {
                        char c = input[posicion];
                        sb.Append(c);
                        if (c == '\n') { linea++; columna = 1; } else columna++;
                        if (c == '\\')
                        {
                            posicion++;
                            if (posicion < input.Length)
                            {
                                sb.Append(input[posicion]);
                                posicion++;
                                columna++;
                            }
                            continue;
                        }

                        if (c == '\'')
                        {
                            closed = true;
                            posicion++;
                            break;
                        }

                        posicion++;
                    }

                    tokens.Add(new TokenInfo(closed ? TokenType.STRING : TokenType.ERROR, sb.ToString(), start, startLine, startCol));
                    continue;
                }

                if (char.IsLetter(actual) || actual == '_')
                {
                    int start = posicion;
                    int startCol = columna;
                    var sb = new StringBuilder();

                    while (posicion < input.Length && (char.IsLetterOrDigit(input[posicion]) || input[posicion] == '_'))
                    {
                        sb.Append(input[posicion]);
                        posicion++;
                        columna++;
                    }

                    var lexema = sb.ToString();
                    var tipo = combinedKeywords.Contains(lexema) ? TokenType.KEYWORD : TokenType.ID;
                    tokens.Add(new TokenInfo(tipo, lexema, start, linea, startCol));
                    continue;
                }

                if (char.IsDigit(actual))
                {
                    int start = posicion;
                    int startCol = columna;
                    var sb = new StringBuilder();
                    bool hasDot = false;
                    bool hasExp = false;

                    while (posicion < input.Length)
                    {
                        char c = input[posicion];
                        if (char.IsDigit(c))
                        {
                            sb.Append(c);
                            posicion++;
                            columna++;
                            continue;
                        }

                        if (!hasDot && c == '.')
                        {
                            if (posicion + 1 < input.Length && input[posicion + 1] == '.')
                            {
                                break;
                            }
                            hasDot = true;
                            sb.Append(c);
                            posicion++;
                            columna++;
                            continue;
                        }

                        if (!hasExp && (c == 'e' || c == 'E'))
                        {
                            hasExp = true;
                            sb.Append(c);
                            posicion++;
                            columna++;
                            if (posicion < input.Length && (input[posicion] == '+' || input[posicion] == '-'))
                            {
                                sb.Append(input[posicion]);
                                posicion++;
                                columna++;
                            }
                            continue;
                        }

                        if (posicion < input.Length && (input[posicion] == 'f' || input[posicion] == 'F' || input[posicion] == 'd' || input[posicion] == 'D' || input[posicion] == 'l' || input[posicion] == 'L'))
                        {
                            sb.Append(input[posicion]);
                            posicion++;
                            columna++;
                            break;
                        }

                        break;
                    }

                    tokens.Add(new TokenInfo(TokenType.NUM, sb.ToString(), start, linea, startCol));
                    continue;
                }

                string two = (posicion + 1 < input.Length) ? input.Substring(posicion, 2) : string.Empty;

                if (two == "//")
                {
                    int start = posicion;
                    int startLine = linea;
                    int startCol = columna;
                    var sb = new StringBuilder();
                    sb.Append("//");
                    posicion += 2;
                    columna += 2;

                    while (posicion < input.Length && input[posicion] != '\n')
                    {
                        sb.Append(input[posicion]);
                        posicion++;
                        columna++;
                    }

                    tokens.Add(new TokenInfo(TokenType.COMMENT, sb.ToString(), start, startLine, startCol));
                    continue;
                }

                if (two == "/*")
                {
                    int start = posicion;
                    int startLine = linea;
                    int startCol = columna;
                    var sb = new StringBuilder();
                    sb.Append("/*");
                    posicion += 2;
                    columna += 2;
                    bool closed = false;

                    while (posicion < input.Length)
                    {
                        if (posicion + 1 < input.Length && input.Substring(posicion, 2) == "*/")
                        {
                            sb.Append("*/");
                            posicion += 2;
                            columna += 2;
                            closed = true;
                            break;
                        }

                        if (input[posicion] == '\n')
                        {
                            sb.Append('\n');
                            posicion++;
                            linea++;
                            columna = 1;
                            continue;
                        }

                        sb.Append(input[posicion]);
                        posicion++;
                        columna++;
                    }

                    tokens.Add(new TokenInfo(closed ? TokenType.COMMENT : TokenType.ERROR, sb.ToString(), start, startLine, startCol));
                    continue;
                }

                var ops = LanguageRules.GetOperators(SelectedLanguage);
                bool matchedOp = false;
                ReadOnlySpan<char> span = input.AsSpan();
                foreach (var op in ops)
                {
                    if (posicion + op.Length <= input.Length && span.Slice(posicion, op.Length).SequenceEqual(op.AsSpan()))
                    {
                        tokens.Add(new TokenInfo(TokenType.OP, op, posicion, linea, columna));
                        posicion += op.Length;
                        columna += op.Length;
                        matchedOp = true;
                        break;
                    }
                }

                if (matchedOp) continue;

                var delims = LanguageRules.GetDelimiters(SelectedLanguage);
                if (delims.IndexOf(actual) >= 0)
                {
                    tokens.Add(new TokenInfo(TokenType.DEL, actual.ToString(), posicion, linea, columna));
                    posicion++;
                    columna++;
                    continue;
                }

                tokens.Add(new TokenInfo(TokenType.ERROR, actual.ToString(), posicion, linea, columna));
                posicion++;
                columna++;
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];
                if (t.Tipo == TokenType.ID && _keywords.Contains(t.Token))
                {
                    tokens[i] = new TokenInfo(TokenType.KEYWORD, t.Token, t.Posicion, t.Linea, t.Columna);
                }
            }

            return tokens;
        }

        public List<Diagnostic> Diagnosticar(string input)
        {
            var detected = DetectLanguageFromContent(input);
            return Diagnosticar(input, detected);
        }

        private Language DetectLanguageFromContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return SelectedLanguage;

            var counts = new Dictionary<Language, int>
            {
                { Language.Kotlin, 0 },
                { Language.CSharp, 0 },
                { Language.Java, 0 }
            };

            foreach (var lang in new[] { Language.Kotlin, Language.CSharp, Language.Java })
            {
                var set = GetKeywordSet(lang);
                int cnt = 0;
                foreach (var kw in set)
                {
                    if (Regex.IsMatch(input, $@"\b{Regex.Escape(kw)}\b", RegexOptions.IgnoreCase)) cnt++;
                }
                counts[lang] = cnt;
            }

            Language best = SelectedLanguage;
            int bestCount = -1;
            foreach (var kv in counts)
            {
                if (kv.Value > bestCount)
                {
                    best = kv.Key;
                    bestCount = kv.Value;
                }
            }

            if (bestCount <= 0) return SelectedLanguage;
            return best;
        }

        private HashSet<string> GetKeywordSet(Language language)
        {
            switch (language)
            {
                case Language.Kotlin:
                    return new HashSet<string>(new[] { "var","val","fun","if","else","when","while","for","return","true","false","class","object","interface","package","import","override","in","is","as" }, StringComparer.OrdinalIgnoreCase);
                case Language.CSharp:
                    return new HashSet<string>(new[] { "var","if","else","while","for","return","true","false","class","namespace","using","public","private","protected","static","void","new","interface","enum","struct","int","string","bool","double","float","decimal","long","short","byte","char" }, StringComparer.OrdinalIgnoreCase);
                case Language.Java:
                    return new HashSet<string>(new[] { "if","else","while","for","return","true","false","class","package","import","public","private","protected","static","void","new","interface","extends","implements","try","catch","int","String","boolean","double","float","long","short","byte","char" }, StringComparer.OrdinalIgnoreCase);
                default:
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private List<TokenInfo> AnalizarTexto(string input)
        {
            var tokens = new List<TokenInfo>();
            int posicion = 0;
            int linea = 1;
            int columna = 1;

            while (posicion < input.Length)
            {
                char actual = input[posicion];

                if (actual == '\n')
                {
                    posicion++;
                    linea++;
                    columna = 1;
                    continue;
                }

                if (char.IsWhiteSpace(actual))
                {
                    posicion++;
                    columna++;
                    continue;
                }

                if (char.IsLetter(actual))
                {
                    int start = posicion;
                    int startCol = columna;
                    var sb = new StringBuilder();

                    while (posicion < input.Length && (char.IsLetter(input[posicion]) || input[posicion] == '\''))
                    {
                        sb.Append(input[posicion]);
                        posicion++;
                        columna++;
                    }

                    tokens.Add(new TokenInfo(TokenType.WORD, sb.ToString(), start, linea, startCol));
                    continue;
                }

                if (char.IsDigit(actual))
                {
                    int start = posicion;
                    int startCol = columna;
                    var sb = new StringBuilder();
                    bool hasDot = false;

                    while (posicion < input.Length && (char.IsDigit(input[posicion]) || (!hasDot && input[posicion] == '.')))
                    {
                        if (input[posicion] == '.') hasDot = true;
                        sb.Append(input[posicion]);
                        posicion++;
                        columna++;
                    }

                    tokens.Add(new TokenInfo(TokenType.NUM, sb.ToString(), start, linea, startCol));
                    continue;
                }

                string punctuations = ".,;:!?\"'()[]{}-\u2013\u2014";
                if (punctuations.IndexOf(actual) >= 0)
                {
                    tokens.Add(new TokenInfo(TokenType.PUNCT, actual.ToString(), posicion, linea, columna));
                    posicion++;
                    columna++;
                    continue;
                }

                tokens.Add(new TokenInfo(TokenType.ERROR, actual.ToString(), posicion, linea, columna));
                posicion++;
                columna++;
            }

            return tokens;
        }
    }
}
