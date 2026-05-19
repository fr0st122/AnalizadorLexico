using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnalizadorLexico_V2
{
    // Módulo de texto: aplica reglas léxicas, sintácticas y gramaticales para escritura cotidiana.
    public sealed class TextAnalysisService
    {
        private static readonly Regex SentenceRegex = new(@"[^.!?]+[.!?]*", RegexOptions.Compiled);
        private static readonly Regex RepeatedWordRegex = new(@"\b(\p{L}+)\s+\1\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex MissingSpaceAfterPunctuationRegex = new(@"[,.!?;:]\p{L}", RegexOptions.Compiled);
        private static readonly Regex SpaceBeforePunctuationRegex = new(@"\s+[,.!?;:]", RegexOptions.Compiled);
        private static readonly Regex RepeatedPunctuationRegex = new(@"([!?.,])\1{2,}", RegexOptions.Compiled);

        public List<Diagnostic> Analyze(string input, IReadOnlyList<TokenInfo> tokens)
        {
            var diagnostics = new List<Diagnostic>();
            if (string.IsNullOrWhiteSpace(input))
            {
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto", "El texto está vacío.", 1, 1));
                return diagnostics;
            }

            AnalyzeLexical(input, tokens, diagnostics);
            AnalyzeSyntax(input, diagnostics);
            AnalyzeGrammar(input, diagnostics);

            return diagnostics;
        }

        private static void AnalyzeLexical(string input, IReadOnlyList<TokenInfo> tokens, List<Diagnostic> diagnostics)
        {
            foreach (var token in tokens.Where(t => t.Tipo == TokenType.ERROR))
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Warning,
                    "Texto léxico",
                    $"Símbolo poco común en texto cotidiano: '{token.Token}'.",
                    token.Linea,
                    token.Columna));
            }

            foreach (Match match in Regex.Matches(input, @" {2,}"))
            {
                var (line, column) = GetLineColumn(input, match.Index);
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto léxico", "Hay espacios repetidos.", line, column));
            }

            foreach (var token in tokens.Where(t => t.Tipo == TokenType.WORD && t.Token.Length > 28))
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Warning,
                    "Texto léxico",
                    $"Palabra muy larga; revisa si está escrita correctamente: '{token.Token}'.",
                    token.Linea,
                    token.Columna));
            }
        }

        private static void AnalyzeSyntax(string input, List<Diagnostic> diagnostics)
        {
            if (HasUnbalanced(input, '(', ')'))
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Texto sintáctico", "Hay paréntesis sin cerrar o sin abrir.", 1, 1));

            if (HasUnbalanced(input, '[', ']'))
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Texto sintáctico", "Hay corchetes sin cerrar o sin abrir.", 1, 1));

            if (input.Count(c => c == '"') % 2 != 0)
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Texto sintáctico", "Hay comillas dobles sin cerrar.", 1, 1));

            foreach (Match match in MissingSpaceAfterPunctuationRegex.Matches(input))
            {
                var (line, column) = GetLineColumn(input, match.Index + 1);
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto sintáctico", "Falta un espacio después del signo de puntuación.", line, column));
            }

            foreach (Match match in SpaceBeforePunctuationRegex.Matches(input))
            {
                var (line, column) = GetLineColumn(input, match.Index);
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto sintáctico", "Sobra un espacio antes del signo de puntuación.", line, column));
            }

            foreach (Match match in RepeatedPunctuationRegex.Matches(input))
            {
                var (line, column) = GetLineColumn(input, match.Index);
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto sintáctico", "Puntuación repetida en exceso.", line, column));
            }

            foreach (Match sentence in SentenceRegex.Matches(input))
            {
                string text = sentence.Value.Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                int start = sentence.Index + sentence.Value.IndexOf(sentence.Value.First(c => !char.IsWhiteSpace(c)));
                char last = text[^1];
                if (last != '.' && last != '!' && last != '?')
                {
                    var (line, column) = GetLineColumn(input, start + text.Length - 1);
                    diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto sintáctico", "La oración parece no terminar con punto, signo de exclamación o interrogación.", line, column));
                }

                char firstLetter = text.FirstOrDefault(char.IsLetter);
                if (firstLetter != '\0' && char.IsLower(firstLetter))
                {
                    var (line, column) = GetLineColumn(input, start);
                    diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto sintáctico", "La oración debería iniciar con mayúscula.", line, column));
                }
            }
        }

        private static void AnalyzeGrammar(string input, List<Diagnostic> diagnostics)
        {
            foreach (Match match in RepeatedWordRegex.Matches(input))
            {
                var (line, column) = GetLineColumn(input, match.Index);
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, "Texto gramatical", "Palabra repetida de forma consecutiva.", line, column));
            }

            AddPatternDiagnostic(input, diagnostics, @"\baver\b", "Texto gramatical", "Posible confusión: usa 'a ver' o 'haber' según el contexto.");
            AddPatternDiagnostic(input, diagnostics, @"\bhabia\b", "Texto gramatical", "Posible falta de tilde: se recomienda 'había'.");
            AddPatternDiagnostic(input, diagnostics, @"\bdespues\b", "Texto gramatical", "Posible falta de tilde: se recomienda 'después'.");
            AddPatternDiagnostic(input, diagnostics, @"\btambien\b", "Texto gramatical", "Posible falta de tilde: se recomienda 'también'.");
            AddPatternDiagnostic(input, diagnostics, @"\besta\b(?=\s+(muy|bien|mal|claro|listo|abierto|cerrado|hecho))", "Texto gramatical", "Posible falta de tilde: usa 'está' cuando viene del verbo estar.");
            AddPatternDiagnostic(input, diagnostics, @"\bhaiga\b", "Texto gramatical", "Forma no recomendada: normalmente se usa 'haya'.");
            AddPatternDiagnostic(input, diagnostics, @"\bmas sin embargo\b", "Texto gramatical", "Expresión redundante: usa 'sin embargo' o 'mas'.");
            AddPatternDiagnostic(input, diagnostics, @"\bsubir arriba\b", "Texto gramatical", "Expresión redundante: 'subir' ya implica dirección hacia arriba.");
            AddPatternDiagnostic(input, diagnostics, @"\bbajar abajo\b", "Texto gramatical", "Expresión redundante: 'bajar' ya implica dirección hacia abajo.");
            AddPatternDiagnostic(input, diagnostics, @"\bla problema\b", "Texto gramatical", "Concordancia: se recomienda 'el problema'.");
            AddPatternDiagnostic(input, diagnostics, @"\bel mano\b", "Texto gramatical", "Concordancia: se recomienda 'la mano'.");
            AddPatternDiagnostic(input, diagnostics, @"\blos casa\b", "Texto gramatical", "Concordancia: se recomienda 'las casas' o 'la casa', según el caso.");
            AddPatternDiagnostic(input, diagnostics, @"\blas perro\b", "Texto gramatical", "Concordancia: se recomienda 'los perros' o 'el perro', según el caso.");
            AddPatternDiagnostic(input, diagnostics, @"\bmucho gracias\b", "Texto gramatical", "Concordancia: se recomienda 'muchas gracias'.");
        }

        private static void AddPatternDiagnostic(string input, List<Diagnostic> diagnostics, string pattern, string source, string message)
        {
            foreach (Match match in Regex.Matches(input, pattern, RegexOptions.IgnoreCase))
            {
                var (line, column) = GetLineColumn(input, match.Index);
                diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, source, message, line, column));
            }
        }

        private static bool HasUnbalanced(string input, char open, char close)
        {
            int balance = 0;
            foreach (char c in input)
            {
                if (c == open) balance++;
                if (c == close) balance--;
                if (balance < 0) return true;
            }

            return balance != 0;
        }

        private static (int line, int column) GetLineColumn(string input, int index)
        {
            int line = 1;
            int column = 1;
            for (int i = 0; i < input.Length && i < index; i++)
            {
                if (input[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }

            return (line, column);
        }
    }
}
