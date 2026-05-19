using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalizadorLexico_V2
{
    // Módulo sintáctico: transforma tokens de código en un AST y reporta errores de estructura.
    public sealed class ParseResult
    {
        public ProgramNode Program { get; } = new();
        public List<Diagnostic> Diagnostics { get; } = new();
        public bool Success => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
    }

    public sealed class Parser
    {
        private readonly List<TokenInfo> _tokens;
        private int _position;
        private readonly ParseResult _result = new();
        private readonly Language _language;

        public Parser(IEnumerable<TokenInfo> tokens, Language language = Language.CSharp)
        {
            _language = language;
            _tokens = tokens
                .Where(t => t.Tipo != TokenType.COMMENT && !string.IsNullOrWhiteSpace(t.Token))
                .ToList();
        }

        public ParseResult ParseProgram()
        {
            while (!IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                {
                    _result.Program.Statements.Add(stmt);
                }
                else
                {
                    Synchronize();
                }
            }

            return _result;
        }

        private StatementNode? ParseStatement()
        {
            if (MatchKeyword("using") || MatchKeyword("import") || MatchKeyword("package")) return ParseDirective(Previous());
            if (MatchKeyword("namespace")) return ParseNamespaceDeclaration(Previous());

            var modifiers = ReadModifiers();
            if (MatchKeyword("class")) return ParseClassDeclaration(Previous(), modifiers);
            if (MatchKeyword("fun")) return ParseFunctionDeclaration(Previous(), modifiers, null);
            if (IsTypedFunctionStart()) return ParseTypedFunctionDeclaration(modifiers);
            if (IsTypedDeclarationStart()) return ParseTypedVarDeclaration();

            if (MatchKeyword("var") || MatchKeyword("val")) return ParseVarDeclaration(Previous());
            if (MatchKeyword("for")) return ParseForStatement(Previous());
            if (MatchKeyword("when")) return ParseWhenStatement(Previous());
            if (MatchKeyword("if")) return ParseIfStatement(Previous());
            if (MatchKeyword("while")) return ParseWhileStatement(Previous());
            if (MatchKeyword("return")) return ParseReturnStatement(Previous());
            if (MatchDelimiter("{")) return ParseBlockStatement(Previous());

            if (Check(TokenType.ID) && CheckNextOperator("="))
            {
                return ParseAssignmentStatement();
            }

            return ParseExpressionStatement();
        }

        private StatementNode ParseDirective(TokenInfo directiveToken)
        {
            bool isStaticImport = directiveToken.Token.Equals("using", StringComparison.OrdinalIgnoreCase)
                && MatchKeyword("static");
            var name = ParseQualifiedName();
            if (isStaticImport)
                name = "static " + name;

            ConsumeDelimiter(";", $"Se esperaba ';' después de '{directiveToken.Token}'.");
            return new DirectiveStatement(directiveToken.Token, name, directiveToken.Linea, directiveToken.Columna);
        }

        private StatementNode ParseNamespaceDeclaration(TokenInfo namespaceToken)
        {
            var name = ParseQualifiedName();
            var members = new List<StatementNode>();

            if (MatchDelimiter("{"))
            {
                while (!IsAtEnd() && !CheckDelimiter("}"))
                {
                    var member = ParseStatement();
                    if (member != null)
                        members.Add(member);
                    else
                        Synchronize();
                }

                ConsumeDelimiter("}", "Se esperaba '}' para cerrar el namespace.");
            }
            else
            {
                ConsumeDelimiter(";", "Se esperaba ';' o '{' después del namespace.");
            }

            return new NamespaceDeclarationStatement(name, members, namespaceToken.Linea, namespaceToken.Columna);
        }

        private string ParseQualifiedName()
        {
            var parts = new List<string>();
            if (Check(TokenType.ID) || Check(TokenType.KEYWORD))
            {
                parts.Add(Advance().Token);
                while (MatchOperator(".") || MatchDelimiter("."))
                {
                    if (Check(TokenType.ID) || Check(TokenType.KEYWORD))
                        parts.Add(Advance().Token);
                    else
                        break;
                }
            }

            return string.Join(".", parts);
        }

        private StatementNode ParseClassDeclaration(TokenInfo classToken, List<string> modifiers)
        {
            var nameToken = Consume(TokenType.ID, "Se esperaba identificador después de 'class'.");

            while (!IsAtEnd() && !CheckDelimiter("{") && !CheckDelimiter(";"))
                Advance();

            var members = new List<StatementNode>();
            if (MatchDelimiter("{"))
            {
                while (!IsAtEnd() && !CheckDelimiter("}"))
                {
                    var member = ParseStatement();
                    if (member != null)
                        members.Add(member);
                    else
                        Synchronize();
                }

                ConsumeDelimiter("}", "Se esperaba '}' para cerrar la clase.");
            }
            else
            {
                ConsumeDelimiter(";", "Se esperaba '{' para iniciar la clase.");
            }

            return new ClassDeclarationStatement(nameToken.Token, modifiers, members, classToken.Linea, classToken.Columna);
        }

        private StatementNode ParseVarDeclaration(TokenInfo varToken)
        {
            var identifier = Consume(TokenType.ID, varToken.Token.Equals("var", StringComparison.OrdinalIgnoreCase) ? "Se esperaba identificador después de 'var'." : "Se esperaba identificador después de 'val'.");
            string? declaredType = null;
            if (MatchDelimiter(":"))
            {
                declaredType = ParseTypeName();
            }
            ExpressionNode? initializer = null;
            if (MatchOperator("="))
            {
                initializer = ParseExpression();
            }

            ConsumeDelimiter(";", "Se esperaba ';' después de la declaración.");
            bool isMutable = string.Equals(varToken.Token, "var", StringComparison.OrdinalIgnoreCase);
            return new VarDeclarationStatement(identifier.Token, declaredType, initializer, isMutable, varToken.Linea, varToken.Columna);
        }

        private StatementNode ParseTypedVarDeclaration()
        {
            var typeToken = Peek();
            string declaredType = ParseTypeName();
            var identifier = Consume(TokenType.ID, $"Se esperaba identificador después del tipo '{typeToken.Token}'.");
            ExpressionNode? initializer = null;

            if (MatchOperator("="))
            {
                initializer = ParseExpression();
            }

            ConsumeDelimiter(";", "Se esperaba ';' después de la declaración.");
            return new VarDeclarationStatement(identifier.Token, declaredType, initializer, true, typeToken.Linea, typeToken.Columna);
        }

        private StatementNode ParseForStatement(TokenInfo forToken)
        {
            ConsumeDelimiter("(", "Se esperaba '(' después de 'for'.");
            string? iterator = null;
            ExpressionNode? iterable = null;

            if (Check(TokenType.ID))
            {
                iterator = Advance().Token;
                if (MatchKeyword("in"))
                {
                    iterable = ParseExpression();
                }
                else
                {
                    iterable = ParseExpression();
                }
            }

            ConsumeDelimiter(")", "Se esperaba ')' en 'for'.");
            var body = ParseStatement() ?? new BlockStatement(forToken.Linea, forToken.Columna);
            return new ForStatement(iterator, iterable, body, forToken.Linea, forToken.Columna);
        }

        private StatementNode ParseWhenStatement(TokenInfo whenToken)
        {
            ExpressionNode? subject = null;
            if (MatchDelimiter("("))
            {
                subject = ParseExpression();
                ConsumeDelimiter(")", "Se esperaba ')' después de subject en 'when'.");
            }

            ConsumeDelimiter("{", "Se esperaba '{' para iniciar 'when'.");
            var whenStmt = new WhenStatement(subject, whenToken.Linea, whenToken.Columna);

            while (!IsAtEnd() && !CheckDelimiter("}"))
            {
                if (MatchKeyword("else"))
                {
                    if (MatchOperator("->"))
                    {
                        var body = ParseStatement() ?? new BlockStatement(whenToken.Linea, whenToken.Columna);
                        whenStmt.Entries.Add(new WhenEntry(null, body));
                        continue;
                    }
                }

                var conditions = new List<ExpressionNode>();
                while (true)
                {
                    var cond = ParseExpression();
                    conditions.Add(cond);
                    if (MatchDelimiter(",")) continue;
                    break;
                }

                if (MatchOperator("->"))
                {
                    var body = ParseStatement() ?? new BlockStatement(whenToken.Linea, whenToken.Columna);
                    whenStmt.Entries.Add(new WhenEntry(conditions, body));
                    continue;
                }

                Synchronize();
            }

            ConsumeDelimiter("}", "Se esperaba '}' para cerrar 'when'.");
            return whenStmt;
        }

        private StatementNode ParseFunctionDeclaration(TokenInfo funToken)
        {
            return ParseFunctionDeclaration(funToken, new List<string>(), null);
        }

        private StatementNode ParseTypedFunctionDeclaration(List<string> modifiers)
        {
            var returnTypeToken = Peek();
            string returnType = ParseTypeName();
            var nameToken = Consume(TokenType.ID, $"Se esperaba identificador después del tipo '{returnTypeToken.Token}'.");
            var parameters = ParseParameterList();
            var body = ParseFunctionBody(nameToken);

            return new FunctionDeclarationStatement(nameToken.Token, returnType, modifiers, parameters, body, returnTypeToken.Linea, returnTypeToken.Columna);
        }

        private StatementNode ParseFunctionDeclaration(TokenInfo funToken, List<string> modifiers, string? returnType)
        {
            var nameToken = Consume(TokenType.ID, "Se esperaba identificador después de 'fun'.");
            var parameters = ParseParameterList();
            var body = ParseFunctionBody(funToken);

            return new FunctionDeclarationStatement(nameToken.Token, returnType, modifiers, parameters, body, funToken.Linea, funToken.Columna);
        }

        private List<string> ParseParameterList()
        {
            var parameters = new List<string>();
            if (MatchDelimiter("("))
            {
                if (!CheckDelimiter(")"))
                {
                    do
                    {
                        TokenInfo p;
                        if (IsTypedParameterStart())
                        {
                            ParseTypeName();
                            p = Consume(TokenType.ID, "Se esperaba identificador de parámetro.");
                        }
                        else
                        {
                            p = Consume(TokenType.ID, "Se esperaba identificador de parámetro.");
                            if (MatchDelimiter(":"))
                                ParseTypeName();
                        }

                        parameters.Add(p.Token);
                    } while (MatchDelimiter(","));
                }
                ConsumeDelimiter(")", "Se esperaba ')' al cerrar lista de parámetros.");
            }

            return parameters;
        }

        private BlockStatement ParseFunctionBody(TokenInfo ownerToken)
        {
            BlockStatement body;
            if (MatchDelimiter("{"))
            {
                body = (BlockStatement)ParseBlockStatement(Previous());
            }
            else if (MatchOperator("="))
            {
                var expr = ParseExpression();
                ConsumeDelimiter(";", "Se esperaba ';' después de la expresión de función.");
                body = new BlockStatement(ownerToken.Linea, ownerToken.Columna);
                body.Statements.Add(new ReturnStatement(expr, ownerToken.Linea, ownerToken.Columna));
            }
            else
            {
                body = new BlockStatement(ownerToken.Linea, ownerToken.Columna);
            }

            return body;
        }

        private StatementNode ParseAssignmentStatement()
        {
            var identifier = Advance();
            ConsumeOperator("=", "Se esperaba '=' en la asignación.");
            var value = ParseExpression();
            ConsumeDelimiter(";", "Se esperaba ';' después de la asignación.");
            return new AssignmentStatement(identifier.Token, value, identifier.Linea, identifier.Columna);
        }

        private StatementNode ParseExpressionStatement()
        {
            var expr = ParseExpression();
            ConsumeDelimiter(";", "Se esperaba ';' al final de la expresión.");
            return new ExpressionStatement(expr, expr.Line, expr.Column);
        }

        private StatementNode ParseIfStatement(TokenInfo ifToken)
        {
            ConsumeDelimiter("(", "Se esperaba '(' después de 'if'.");
            var condition = ParseExpression();
            ConsumeDelimiter(")", "Se esperaba ')' después de la condición.");
            var thenBranch = ParseStatement() ?? new BlockStatement(ifToken.Linea, ifToken.Columna);
            StatementNode? elseBranch = null;

            if (MatchKeyword("else"))
            {
                elseBranch = ParseStatement() ?? new BlockStatement(ifToken.Linea, ifToken.Columna);
            }

            return new IfStatement(condition, thenBranch, elseBranch, ifToken.Linea, ifToken.Columna);
        }

        private StatementNode ParseWhileStatement(TokenInfo whileToken)
        {
            ConsumeDelimiter("(", "Se esperaba '(' después de 'while'.");
            var condition = ParseExpression();
            ConsumeDelimiter(")", "Se esperaba ')' después de la condición.");
            var body = ParseStatement() ?? new BlockStatement(whileToken.Linea, whileToken.Columna);
            return new WhileStatement(condition, body, whileToken.Linea, whileToken.Columna);
        }

        private StatementNode ParseReturnStatement(TokenInfo returnToken)
        {
            ExpressionNode? expr = null;
            if (!CheckDelimiter(";"))
            {
                expr = ParseExpression();
            }
            ConsumeDelimiter(";", "Se esperaba ';' después de 'return'.");
            return new ReturnStatement(expr, returnToken.Linea, returnToken.Columna);
        }

        private StatementNode ParseBlockStatement(TokenInfo openBrace)
        {
            var block = new BlockStatement(openBrace.Linea, openBrace.Columna);
            while (!IsAtEnd() && !CheckDelimiter("}"))
            {
                var stmt = ParseStatement();
                if (stmt != null)
                {
                    block.Statements.Add(stmt);
                }
                else
                {
                    Synchronize();
                }
            }

            ConsumeDelimiter("}", "Se esperaba '}' para cerrar bloque.");
            return block;
        }

        private ExpressionNode ParseExpression() => ParseOr();

        // Precedencia de menor a mayor: ||, &&, igualdad, comparación, suma, producto, unario, postfix.
        private ExpressionNode ParseOr()
        {
            var expr = ParseAnd();
            while (MatchOperator("||"))
            {
                var op = Previous();
                var right = ParseAnd();
                expr = new BinaryExpression(op.Token, expr, right, op.Linea, op.Columna);
            }
            return expr;
        }

        private ExpressionNode ParseAnd()
        {
            var expr = ParseEquality();
            while (MatchOperator("&&"))
            {
                var op = Previous();
                var right = ParseEquality();
                expr = new BinaryExpression(op.Token, expr, right, op.Linea, op.Columna);
            }
            return expr;
        }

        private ExpressionNode ParseEquality()
        {
            var expr = ParseComparison();
            while (MatchOperator("==") || MatchOperator("!="))
            {
                var op = Previous();
                var right = ParseComparison();
                expr = new BinaryExpression(op.Token, expr, right, op.Linea, op.Columna);
            }
            return expr;
        }

        private ExpressionNode ParseComparison()
        {
            var expr = ParseTerm();
            while (MatchOperator("<") || MatchOperator("<=") || MatchOperator(">") || MatchOperator(">="))
            {
                var op = Previous();
                var right = ParseTerm();
                expr = new BinaryExpression(op.Token, expr, right, op.Linea, op.Columna);
            }
            return expr;
        }

        private ExpressionNode ParseTerm()
        {
            var expr = ParseFactor();
            while (MatchOperator("+") || MatchOperator("-"))
            {
                var op = Previous();
                var right = ParseFactor();
                expr = new BinaryExpression(op.Token, expr, right, op.Linea, op.Columna);
            }
            return expr;
        }

        private ExpressionNode ParseFactor()
        {
            var expr = ParseUnary();
            while (MatchOperator("*") || MatchOperator("/"))
            {
                var op = Previous();
                var right = ParseUnary();
                expr = new BinaryExpression(op.Token, expr, right, op.Linea, op.Columna);
            }
            return expr;
        }

        private ExpressionNode ParseUnary()
        {
            if (MatchOperator("!") || MatchOperator("-"))
            {
                var op = Previous();
                var right = ParseUnary();
                return new UnaryExpression(op.Token, right, op.Linea, op.Columna);
            }

            return ParsePostfix();
        }

        private ExpressionNode ParsePostfix()
        {
            var expr = ParsePrimary();

            while (true)
            {
                if (MatchOperator(".") || MatchDelimiter("."))
                {
                    var dot = Previous();
                    var member = Consume(TokenType.ID, "Se esperaba identificador después de '.'.");
                    expr = new MemberAccessExpression(expr, member.Token, dot.Linea, dot.Columna);
                    continue;
                }

                if (MatchDelimiter("("))
                {
                    var openParen = Previous();
                    var arguments = new List<ExpressionNode>();
                    if (!CheckDelimiter(")"))
                    {
                        do
                        {
                            arguments.Add(ParseExpression());
                        } while (MatchDelimiter(","));
                    }

                    ConsumeDelimiter(")", "Se esperaba ')' al cerrar argumentos.");
                    expr = new CallExpression(expr, arguments, openParen.Linea, openParen.Columna);
                    continue;
                }

                break;
            }

            return expr;
        }

        private ExpressionNode ParsePrimary()
        {
            if (Match(TokenType.NUM))
            {
                var token = Previous();
                if (double.TryParse(token.Token, out double n))
                {
                    return new LiteralExpression(n, token.Linea, token.Columna);
                }
                return new LiteralExpression(token.Token, token.Linea, token.Columna);
            }

            if (Match(TokenType.STRING))
            {
                var token = Previous();
                return new LiteralExpression(token.Token, token.Linea, token.Columna);
            }

            if (MatchKeyword("true"))
            {
                var token = Previous();
                return new LiteralExpression(true, token.Linea, token.Columna);
            }

            if (MatchKeyword("false"))
            {
                var token = Previous();
                return new LiteralExpression(false, token.Linea, token.Columna);
            }

            if (Match(TokenType.ID))
            {
                var token = Previous();
                return new VariableExpression(token.Token, token.Linea, token.Columna);
            }

            if (MatchDelimiter("("))
            {
                var open = Previous();
                var expr = ParseExpression();
                ConsumeDelimiter(")", "Se esperaba ')' para cerrar la expresión.");
                return expr;
            }

            var unexpected = Peek();
            _result.Diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                "Sintáctico",
                $"Token inesperado '{unexpected.Token}'.",
                unexpected.Linea,
                unexpected.Columna));
            Advance();
            return new LiteralExpression(null, unexpected.Linea, unexpected.Columna);
        }

        private bool Match(TokenType type)
        {
            if (!Check(type)) return false;
            Advance();
            return true;
        }

        private bool MatchKeyword(string lexeme)
        {
            if (IsAtEnd()) return false;
            var p = Peek();
            if (p.Tipo != TokenType.KEYWORD && p.Tipo != TokenType.ID) return false;
            if (!string.Equals(p.Token, lexeme, StringComparison.OrdinalIgnoreCase)) return false;
            Advance();
            return true;
        }

        private bool MatchDelimiter(string lexeme)
        {
            if (!Check(TokenType.DEL)) return false;
            if (!string.Equals(Peek().Token, lexeme, StringComparison.Ordinal)) return false;
            Advance();
            return true;
        }

        private bool MatchOperator(string lexeme)
        {
            if (!Check(TokenType.OP)) return false;
            if (!string.Equals(Peek().Token, lexeme, StringComparison.Ordinal)) return false;
            Advance();
            return true;
        }

        private TokenInfo Consume(TokenType type, string errorMessage)
        {
            if (Check(type)) return Advance();
            var token = Peek();
            _result.Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Sintáctico", errorMessage, token.Linea, token.Columna));
            return token;
        }

        private TokenInfo ConsumeDelimiter(string lexeme, string errorMessage)
        {
            if (lexeme == ";" && !LanguageRules.SemicolonRequired(_language))
            {
                if (CheckDelimiter(lexeme)) return Advance();
                return Peek();
            }

            if (CheckDelimiter(lexeme)) return Advance();
            var token = Peek();
            _result.Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Sintáctico", errorMessage, token.Linea, token.Columna));
            return token;
        }

        private TokenInfo ConsumeOperator(string lexeme, string errorMessage)
        {
            if (CheckOperator(lexeme)) return Advance();
            var token = Peek();
            _result.Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Sintáctico", errorMessage, token.Linea, token.Columna));
            return token;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Tipo == type;
        }

        private bool CheckDelimiter(string lexeme) => CheckOperatorOrDelimiter(TokenType.DEL, lexeme);
        private bool CheckOperator(string lexeme) => CheckOperatorOrDelimiter(TokenType.OP, lexeme);

        private bool CheckOperatorOrDelimiter(TokenType type, string lexeme)
        {
            if (!Check(type)) return false;
            return string.Equals(Peek().Token, lexeme, StringComparison.Ordinal);
        }

        private bool CheckNextOperator(string lexeme)
        {
            if (_position + 1 >= _tokens.Count) return false;
            var next = _tokens[_position + 1];
            return next.Tipo == TokenType.OP && string.Equals(next.Token, lexeme, StringComparison.Ordinal);
        }

        private bool IsTypedDeclarationStart()
        {
            if (_language == Language.Kotlin) return false;

            int next = GetPositionAfterType(_position);
            return next >= 0 && next < _tokens.Count && _tokens[next].Tipo == TokenType.ID;
        }

        private bool IsTypedFunctionStart()
        {
            if (_language == Language.Kotlin) return false;

            int next = GetPositionAfterType(_position);
            if (next < 0 || next >= _tokens.Count || _tokens[next].Tipo != TokenType.ID) return false;
            int afterName = next + 1;
            return afterName < _tokens.Count
                && _tokens[afterName].Tipo == TokenType.DEL
                && _tokens[afterName].Token == "(";
        }

        private bool IsTypedParameterStart()
        {
            int namePosition = GetPositionAfterType(_position);
            return namePosition >= 0 && namePosition < _tokens.Count && _tokens[namePosition].Tipo == TokenType.ID;
        }

        private List<string> ReadModifiers()
        {
            var modifiers = new List<string>();
            while (!IsAtEnd() && IsModifier(Peek().Token))
                modifiers.Add(Advance().Token);

            return modifiers;
        }

        private static bool IsModifier(string token)
        {
            return token.Equals("public", StringComparison.OrdinalIgnoreCase)
                || token.Equals("private", StringComparison.OrdinalIgnoreCase)
                || token.Equals("protected", StringComparison.OrdinalIgnoreCase)
                || token.Equals("internal", StringComparison.OrdinalIgnoreCase)
                || token.Equals("static", StringComparison.OrdinalIgnoreCase)
                || token.Equals("abstract", StringComparison.OrdinalIgnoreCase)
                || token.Equals("final", StringComparison.OrdinalIgnoreCase)
                || token.Equals("override", StringComparison.OrdinalIgnoreCase);
        }

        private string ParseTypeName()
        {
            var typeToken = Advance();
            var typeName = typeToken.Token;

            while (MatchDelimiter("["))
            {
                ConsumeDelimiter("]", "Se esperaba ']' después de '[' en el tipo.");
                typeName += "[]";
            }

            return typeName;
        }

        private int GetPositionAfterType(int start)
        {
            if (start >= _tokens.Count || !IsKnownTypeName(_tokens[start].Token))
                return -1;

            int position = start + 1;
            while (position + 1 < _tokens.Count
                && _tokens[position].Tipo == TokenType.DEL
                && _tokens[position].Token == "["
                && _tokens[position + 1].Tipo == TokenType.DEL
                && _tokens[position + 1].Token == "]")
            {
                position += 2;
            }

            return position;
        }

        private static bool IsKnownTypeName(string token)
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
                || token.Equals("char", StringComparison.OrdinalIgnoreCase)
                || token.Equals("void", StringComparison.OrdinalIgnoreCase);
        }

        private TokenInfo Advance()
        {
            if (!IsAtEnd()) _position++;
            return Previous();
        }

        private bool IsAtEnd() => _position >= _tokens.Count;

        private TokenInfo Peek()
        {
            if (_tokens.Count == 0)
                return new TokenInfo(TokenType.ERROR, "<EOF>", 0, 1, 1);

            if (IsAtEnd()) return _tokens[^1];
            return _tokens[_position];
        }

        private TokenInfo Previous()
        {
            if (_position == 0) return _tokens.Count > 0 ? _tokens[0] : new TokenInfo(TokenType.ERROR, "<EOF>", 0, 1, 1);
            return _tokens[_position - 1];
        }

        private void Synchronize()
        {
            // Avanza hasta un límite estable para evitar cascadas de errores sintácticos.
            while (!IsAtEnd())
            {
                if (Previous().Tipo == TokenType.DEL && Previous().Token == ";")
                    return;

                if (Peek().Tipo == TokenType.KEYWORD)
                    return;

                if (Peek().Tipo == TokenType.DEL)
                {
                    var tok = Peek();
                    if (tok.Token == "{" || tok.Token == "}" || tok.Token == "," || tok.Token == ")")
                        return;
                }

                if (Peek().Tipo == TokenType.OP && (Peek().Token == "->" || Peek().Token == "=>"))
                    return;

                Advance();
            }
        }
    }
}
