using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalizadorLexico_V2
{
    // Módulo de resumen: estima el lenguaje y genera una explicación legible del análisis.
    public sealed class CodeInsightsResult
    {
        public string Language { get; }
        public int ConfidencePercent { get; }
        public string Summary { get; }

        public CodeInsightsResult(string language, int confidencePercent, string summary)
        {
            Language = language;
            ConfidencePercent = confidencePercent;
            Summary = summary;
        }
    }

    public sealed class CodeInsightsService
    {
        private const int MaxSummaryActions = 18;

        public CodeInsightsResult BuildInsights(string input, InputMode mode, List<TokenInfo> tokens, ParseResult? parseResult, string? selectedLanguage = null)
        {
            (string language, int confidence) = string.IsNullOrWhiteSpace(selectedLanguage)
                ? DetectLanguage(input, mode, tokens)
                : (selectedLanguage.Trim(), 100);
            string summary = BuildSummary(input, mode, tokens, parseResult);
            return new CodeInsightsResult(language, confidence, summary);
        }

        private (string language, int confidence) DetectLanguage(string input, InputMode mode, List<TokenInfo> tokens)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ("Sin contenido", 0);

            if (LooksLikeJson(input))
                return ("JSON", 94);

            if (LooksLikeHtml(input))
                return ("HTML/XML", 90);

            if (mode == InputMode.Texto)
                return ("Texto natural", 95);

            var scores = new Dictionary<string, int>
            {
                ["C#"] = 0,
                ["JavaScript/TypeScript"] = 0,
                ["Python"] = 0,
                ["Java"] = 0,
                ["C/C++"] = 0,
                ["SQL"] = 0
            };

            AddScore(scores, "C#", input, @"\busing\s+[A-Za-z0-9_.]+\s*;", 12);
            AddScore(scores, "C#", input, @"\bnamespace\s+[A-Za-z0-9_.]+", 11);
            AddScore(scores, "C#", input, @"\bpublic\s+(class|interface|enum)\b", 8);
            AddScore(scores, "C#", input, @"\bConsole\.Write(Line)?\b", 8);
            AddScore(scores, "C#", input, @"\b(var|int|string|bool|double)\s+[A-Za-z_][A-Za-z0-9_]*\s*=", 7);

            AddScore(scores, "JavaScript/TypeScript", input, @"\bfunction\s+[A-Za-z_][A-Za-z0-9_]*\s*\(", 9);
            AddScore(scores, "JavaScript/TypeScript", input, @"\b(const|let|var)\s+[A-Za-z_][A-Za-z0-9_]*", 8);
            AddScore(scores, "JavaScript/TypeScript", input, @"=>", 7);
            AddScore(scores, "JavaScript/TypeScript", input, @"\bconsole\.log\s*\(", 8);
            AddScore(scores, "JavaScript/TypeScript", input, @"\b(import|export)\s+", 7);
            AddScore(scores, "JavaScript/TypeScript", input, @"\b(interface|type)\s+[A-Za-z_][A-Za-z0-9_]*", 7);

            AddScore(scores, "Python", input, @"\bdef\s+[A-Za-z_][A-Za-z0-9_]*\s*\(", 9);
            AddScore(scores, "Python", input, @"\bimport\s+[A-Za-z_][A-Za-z0-9_.]*", 8);
            AddScore(scores, "Python", input, @"\bfrom\s+[A-Za-z_][A-Za-z0-9_.]*\s+import\b", 8);
            AddScore(scores, "Python", input, @"^\s*(if|for|while|elif|else)\b.+:\s*$", 6);
            AddScore(scores, "Python", input, @"\bprint\s*\(", 7);

            AddScore(scores, "Java", input, @"\bpublic\s+class\s+[A-Za-z_][A-Za-z0-9_]*", 10);
            AddScore(scores, "Java", input, @"\bpublic\s+static\s+void\s+main\s*\(", 10);
            AddScore(scores, "Java", input, @"\bSystem\.out\.println\s*\(", 9);
            AddScore(scores, "Java", input, @"\bpackage\s+[A-Za-z0-9_.]+\s*;", 8);
            AddScore(scores, "Java", input, @"\bnew\s+[A-Za-z_][A-Za-z0-9_]*\s*\(", 6);

            AddScore(scores, "C/C++", input, @"#include\s*<[^>]+>", 11);
            AddScore(scores, "C/C++", input, @"\b(int|float|double|char|void)\s+main\s*\(", 10);
            AddScore(scores, "C/C++", input, @"\bstd::", 9);
            AddScore(scores, "C/C++", input, @"\bprintf\s*\(", 8);
            AddScore(scores, "C/C++", input, @"\bscanf\s*\(", 8);
            AddScore(scores, "C/C++", input, @"->", 6);

            AddScore(scores, "SQL", input, @"\b(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|JOIN|GROUP\s+BY|ORDER\s+BY)\b", 9);
            AddScore(scores, "SQL", input, @"\bCREATE\s+(TABLE|DATABASE|INDEX)\b", 10);

            int semicolons = tokens.Count(t => t.Tipo == TokenType.DEL && t.Token == ";");
            int braces = tokens.Count(t => t.Tipo == TokenType.DEL && (t.Token == "{" || t.Token == "}"));
            if (semicolons >= 2 && braces >= 2)
            {
                scores["C#"] += 7;
                scores["Java"] += 7;
                scores["C/C++"] += 7;
            }

            if (tokens.Any(t => t.Tipo == TokenType.KEYWORD && t.Token == "var"))
            {
                scores["C#"] += 8;
                scores["JavaScript/TypeScript"] += 6;
            }

            var ranked = scores.OrderByDescending(kv => kv.Value).ToList();
            string bestLanguage = ranked[0].Key;
            int bestScore = ranked[0].Value;
            int secondScore = ranked.Count > 1 ? ranked[1].Value : 0;

            if (bestScore == 0)
                return ("Lenguaje no identificado", 25);

            int spread = Math.Max(0, bestScore - secondScore);
            int confidence = Math.Clamp(55 + (spread * 4), 35, 99);
            return (bestLanguage, confidence);
        }

        private static bool LooksLikeJson(string input)
        {
            string trimmed = input.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return false;

            bool validShape =
                (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                (trimmed.StartsWith("[") && trimmed.EndsWith("]"));

            if (!validShape) return false;
            return Regex.IsMatch(trimmed, "\"[^\"]+\"\\s*:");
        }

        private static bool LooksLikeHtml(string input)
        {
            return Regex.IsMatch(input, @"</?[a-zA-Z][a-zA-Z0-9]*(\s+[^>]*)?>");
        }

        private static void AddScore(Dictionary<string, int> scores, string lang, string input, string pattern, int weight)
        {
            var matches = Regex.Matches(input, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (matches.Count > 0)
                scores[lang] += matches.Count * weight;
        }

        private string BuildSummary(string input, InputMode mode, List<TokenInfo> tokens, ParseResult? parseResult)
        {
            int lines = string.IsNullOrEmpty(input) ? 0 : input.Replace("\r\n", "\n").Split('\n').Length;
            int codeTokens = tokens.Count(t => t.Tipo != TokenType.COMMENT);
            int comments = tokens.Count(t => t.Tipo == TokenType.COMMENT);
            int declarations = tokens.Count(t => t.Tipo == TokenType.KEYWORD && (t.Token == "var" || t.Token == "val" || IsTypeKeyword(t.Token)));
            int conditionals = tokens.Count(t => t.Tipo == TokenType.KEYWORD && (t.Token == "if" || t.Token == "else"));
            int loops = tokens.Count(t => t.Tipo == TokenType.KEYWORD && (t.Token == "while" || t.Token == "for"));
            int returns = tokens.Count(t => t.Tipo == TokenType.KEYWORD && t.Token == "return");

            var sb = new StringBuilder();
            sb.AppendLine("Resumen automático:");
            sb.AppendLine($"- Modo detectado: {(mode == InputMode.Codigo ? "Código" : "Texto")}.");
            sb.AppendLine($"- Líneas analizadas: {lines}.");
            sb.AppendLine($"- Tokens útiles: {codeTokens} (comentarios: {comments}).");

            if (mode == InputMode.Texto)
            {
                int words = tokens.Count(t => t.Tipo == TokenType.WORD);
                int numbers = tokens.Count(t => t.Tipo == TokenType.NUM);
                int punctuation = tokens.Count(t => t.Tipo == TokenType.PUNCT);
                int sentenceCount = Regex.Matches(input, @"[.!?]").Count;
                double averageWordsPerSentence = sentenceCount == 0 ? words : (double)words / sentenceCount;
                sb.AppendLine($"- Contenido textual con {words} palabras y {numbers} números.");
                sb.AppendLine($"- Signos de puntuación detectados: {punctuation}.");
                sb.AppendLine($"- Oraciones estimadas: {Math.Max(1, sentenceCount)}.");
                sb.AppendLine($"- Promedio aproximado: {averageWordsPerSentence:0.0} palabras por oración.");
                sb.AppendLine("- Se ejecutó análisis léxico, sintáctico y gramatical de texto cotidiano.");
                return sb.ToString();
            }

            sb.AppendLine($"- Declaraciones de variables: {declarations}.");
            sb.AppendLine($"- Condicionales detectados: {conditionals}.");
            sb.AppendLine($"- Bucles detectados: {loops}.");
            sb.AppendLine($"- Sentencias return: {returns}.");

            if (parseResult == null)
            {
                sb.AppendLine("- El árbol sintáctico no estuvo disponible.");
                return sb.ToString();
            }

            int syntaxErrors = parseResult.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
            if (syntaxErrors == 0)
            {
                sb.AppendLine("- La estructura sintáctica principal es válida.");
            }
            else
            {
                sb.AppendLine($"- Se detectaron {syntaxErrors} errores sintácticos; el resumen funcional puede ser parcial.");
            }

            var actions = DescribeProgramFlow(parseResult.Program);
            if (actions.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("¿Qué hace cada parte del código?");
                foreach (var action in actions)
                    sb.AppendLine($"- {action}");
            }

            return sb.ToString();
        }

        private List<string> DescribeProgramFlow(ProgramNode program)
        {
            var actions = new List<string>();
            foreach (var statement in program.Statements)
            {
                DescribeStatement(statement, actions, 0);
                if (actions.Count >= MaxSummaryActions)
                    break;
            }

            if (actions.Count >= MaxSummaryActions)
            {
                actions.Add("... (resumen recortado para mantener la lectura clara).");
            }

            return actions;
        }

        private void DescribeStatement(StatementNode statement, List<string> actions, int depth)
        {
            if (actions.Count >= MaxSummaryActions) return;
            string indent = depth > 0 ? new string(' ', depth * 2) : string.Empty;

            switch (statement)
            {
                case DirectiveStatement directive:
                    {
                        actions.Add($"{indent}Declara `{directive.Kind}` para `{directive.Name}`.");
                        break;
                    }
                case NamespaceDeclarationStatement namespaceDecl:
                    {
                        actions.Add($"{indent}Agrupa código en el namespace `{namespaceDecl.Name}`.");
                        foreach (var member in namespaceDecl.Members.Take(4))
                        {
                            DescribeStatement(member, actions, depth + 1);
                            if (actions.Count >= MaxSummaryActions) break;
                        }
                        break;
                    }
                case ClassDeclarationStatement classDecl:
                    {
                        actions.Add($"{indent}Define la clase `{classDecl.Name}` con {classDecl.Members.Count} miembro(s).");
                        foreach (var member in classDecl.Members.Take(4))
                        {
                            DescribeStatement(member, actions, depth + 1);
                            if (actions.Count >= MaxSummaryActions) break;
                        }
                        break;
                    }
                case VarDeclarationStatement varDecl:
                    {
                        string type = string.IsNullOrWhiteSpace(varDecl.DeclaredType)
                            ? string.Empty
                            : $" de tipo `{varDecl.DeclaredType}`";
                        string init = varDecl.Initializer == null
                            ? "sin valor inicial."
                            : $"con valor inicial {DescribeExpression(varDecl.Initializer)}.";
                        actions.Add($"{indent}Declara la variable `{varDecl.Identifier}`{type} {init}");
                        break;
                    }
                case FunctionDeclarationStatement functionDecl:
                    {
                        string returnType = string.IsNullOrWhiteSpace(functionDecl.ReturnType)
                            ? string.Empty
                            : $" que devuelve `{functionDecl.ReturnType}`";
                        actions.Add($"{indent}Define la función `{functionDecl.Name}` con {functionDecl.Parameters.Count} parámetro(s){returnType}.");
                        DescribeStatement(functionDecl.Body, actions, depth + 1);
                        break;
                    }
                case AssignmentStatement assignment:
                    {
                        actions.Add($"{indent}Actualiza `{assignment.Identifier}` con {DescribeExpression(assignment.Value)}.");
                        break;
                    }
                case ExpressionStatement expressionStatement:
                    {
                        actions.Add($"{indent}Evalúa {DescribeExpression(expressionStatement.Expression)}.");
                        break;
                    }
                case IfStatement ifStatement:
                    {
                        actions.Add($"{indent}Evalúa un `if` con condición {DescribeExpression(ifStatement.Condition)}.");
                        actions.Add($"{indent}Si la condición es verdadera, ejecuta:");
                        DescribeStatement(ifStatement.ThenBranch, actions, depth + 1);

                        if (ifStatement.ElseBranch != null && actions.Count < MaxSummaryActions)
                        {
                            actions.Add($"{indent}Si no se cumple, ejecuta la rama `else`:");
                            DescribeStatement(ifStatement.ElseBranch, actions, depth + 1);
                        }
                        break;
                    }
                case WhileStatement whileStatement:
                    {
                        actions.Add($"{indent}Ejecuta un bucle `while` mientras {DescribeExpression(whileStatement.Condition)}.");
                        DescribeStatement(whileStatement.Body, actions, depth + 1);
                        break;
                    }
                case ReturnStatement returnStatement:
                    {
                        if (returnStatement.Expression == null)
                            actions.Add($"{indent}Finaliza con `return`.");
                        else
                            actions.Add($"{indent}Devuelve {DescribeExpression(returnStatement.Expression)}.");
                        break;
                    }
                case BlockStatement block:
                    {
                        actions.Add($"{indent}Entra a un bloque con {block.Statements.Count} instrucción(es).");
                        foreach (var inner in block.Statements.Take(4))
                        {
                            DescribeStatement(inner, actions, depth + 1);
                            if (actions.Count >= MaxSummaryActions) break;
                        }

                        int remaining = block.Statements.Count - 4;
                        if (remaining > 0 && actions.Count < MaxSummaryActions)
                        {
                            actions.Add($"{indent}... y {remaining} instrucción(es) adicionales en ese bloque.");
                        }
                        break;
                    }
            }
        }

        private string DescribeExpression(ExpressionNode expression, int depth = 0)
        {
            if (depth >= 3) return "una expresión compuesta";

            return expression switch
            {
                LiteralExpression literal => DescribeLiteral(literal.Value),
                VariableExpression variable => $"la variable `{variable.Name}`",
                MemberAccessExpression memberAccess => $"{DescribeExpression(memberAccess.Target, depth + 1)}.`{memberAccess.MemberName}`",
                CallExpression call => $"llama a {DescribeExpression(call.Callee, depth + 1)} con {call.Arguments.Count} argumento(s)",
                UnaryExpression unary => $"`{unary.Operator}` aplicado a {DescribeExpression(unary.Operand, depth + 1)}",
                BinaryExpression binary => $"{DescribeExpression(binary.Left, depth + 1)} `{binary.Operator}` {DescribeExpression(binary.Right, depth + 1)}",
                _ => "una expresión"
            };
        }

        private static bool IsTypeKeyword(string token)
        {
            return token.Equals("int", StringComparison.OrdinalIgnoreCase)
                || token.Equals("string", StringComparison.OrdinalIgnoreCase)
                || token.Equals("String", StringComparison.OrdinalIgnoreCase)
                || token.Equals("bool", StringComparison.OrdinalIgnoreCase)
                || token.Equals("boolean", StringComparison.OrdinalIgnoreCase)
                || token.Equals("double", StringComparison.OrdinalIgnoreCase)
                || token.Equals("float", StringComparison.OrdinalIgnoreCase)
                || token.Equals("decimal", StringComparison.OrdinalIgnoreCase)
                || token.Equals("long", StringComparison.OrdinalIgnoreCase)
                || token.Equals("short", StringComparison.OrdinalIgnoreCase)
                || token.Equals("byte", StringComparison.OrdinalIgnoreCase)
                || token.Equals("char", StringComparison.OrdinalIgnoreCase);
        }

        private static string DescribeLiteral(object? value)
        {
            return value switch
            {
                null => "valor nulo",
                bool b => b ? "el valor `true`" : "el valor `false`",
                string s => $"el texto {ToShortString(s)}",
                _ => $"el valor `{value}`"
            };
        }

        private static string ToShortString(string text)
        {
            string clean = text.Replace("\r", " ").Replace("\n", " ");
            if (clean.Length > 30)
                clean = clean[..30] + "...";

            return $"\"{clean}\"";
        }
    }
}
