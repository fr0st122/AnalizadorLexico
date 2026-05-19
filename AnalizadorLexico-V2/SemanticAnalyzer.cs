using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalizadorLexico_V2
{
    // Módulo semántico: valida símbolos, ámbitos y compatibilidad básica de tipos.
    public enum ValueTypeSymbol
    {
        Unknown,
        Number,
        String,
        Bool,
        Void
    }

    public sealed class SemanticAnalyzer
    {
        private readonly List<Diagnostic> _diagnostics = new();
        private readonly Stack<Dictionary<string, ValueTypeSymbol>> _scopes = new();

        public List<Diagnostic> Analyze(ProgramNode program)
        {
            _diagnostics.Clear();
            _scopes.Clear();
            EnterScope();

            foreach (var stmt in program.Statements)
            {
                AnalyzeStatement(stmt);
            }

            ExitScope();
            return _diagnostics.ToList();
        }

        private void AnalyzeStatement(StatementNode statement)
        {
            switch (statement)
            {
                case BlockStatement block:
                    EnterScope();
                    foreach (var stmt in block.Statements)
                    {
                        AnalyzeStatement(stmt);
                    }
                    ExitScope();
                    break;

                case VarDeclarationStatement varDecl:
                    AnalyzeVarDeclaration(varDecl);
                    break;
                case ClassDeclarationStatement classDecl:
                    AnalyzeClassDeclaration(classDecl);
                    break;
                case DirectiveStatement:
                    break;
                case NamespaceDeclarationStatement namespaceDecl:
                    AnalyzeNamespaceDeclaration(namespaceDecl);
                    break;
                case FunctionDeclarationStatement funcDecl:
                    AnalyzeFunctionDeclaration(funcDecl);
                    break;

                case AssignmentStatement assignment:
                    AnalyzeAssignment(assignment);
                    break;

                case ExpressionStatement expressionStatement:
                    AnalyzeExpression(expressionStatement.Expression);
                    break;

                case IfStatement ifStatement:
                    AnalyzeConditional(ifStatement.Condition, ifStatement.Line, ifStatement.Column);
                    AnalyzeStatement(ifStatement.ThenBranch);
                    if (ifStatement.ElseBranch != null)
                        AnalyzeStatement(ifStatement.ElseBranch);
                    break;

                case WhileStatement whileStatement:
                    AnalyzeConditional(whileStatement.Condition, whileStatement.Line, whileStatement.Column);
                    AnalyzeStatement(whileStatement.Body);
                    break;

                case ForStatement forStatement:
                    AnalyzeForStatement(forStatement);
                    break;

                case WhenStatement whenStatement:
                    AnalyzeWhenStatement(whenStatement);
                    break;

                case ReturnStatement returnStatement:
                    if (returnStatement.Expression != null)
                        AnalyzeExpression(returnStatement.Expression);
                    break;
            }
        }

        private void AnalyzeVarDeclaration(VarDeclarationStatement varDecl)
        {
            var current = _scopes.Peek();
            if (current.ContainsKey(varDecl.Identifier))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "Semántico",
                    $"La variable '{varDecl.Identifier}' ya fue declarada en este bloque.",
                    varDecl.Line,
                    varDecl.Column));
                return;
            }

            var declaredType = GetDeclaredType(varDecl.DeclaredType);
            var type = declaredType;
            if (varDecl.Initializer != null)
            {
                var initializerType = AnalyzeExpression(varDecl.Initializer);
                if (declaredType == ValueTypeSymbol.Unknown)
                {
                    type = initializerType;
                }
                else if (initializerType != ValueTypeSymbol.Unknown && declaredType != initializerType)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        "Semántico",
                        $"No se puede asignar '{initializerType}' a variable '{varDecl.Identifier}' de tipo '{declaredType}'.",
                        varDecl.Line,
                        varDecl.Column));
                }
            }

            current[varDecl.Identifier] = type;
        }

        private void AnalyzeClassDeclaration(ClassDeclarationStatement classDecl)
        {
            var current = _scopes.Peek();
            if (current.ContainsKey(classDecl.Name))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "Semántico",
                    $"El identificador '{classDecl.Name}' ya fue declarado.",
                    classDecl.Line,
                    classDecl.Column));
                return;
            }

            current[classDecl.Name] = ValueTypeSymbol.Unknown;

            EnterScope();
            foreach (var member in classDecl.Members)
                AnalyzeStatement(member);
            ExitScope();
        }

        private void AnalyzeNamespaceDeclaration(NamespaceDeclarationStatement namespaceDecl)
        {
            EnterScope();
            foreach (var member in namespaceDecl.Members)
                AnalyzeStatement(member);
            ExitScope();
        }

        private void AnalyzeForStatement(ForStatement forStatement)
        {
            if (forStatement.Iterable != null)
                AnalyzeExpression(forStatement.Iterable);

            EnterScope();
            if (!string.IsNullOrWhiteSpace(forStatement.Iterator))
                _scopes.Peek()[forStatement.Iterator] = ValueTypeSymbol.Unknown;

            AnalyzeStatement(forStatement.Body);
            ExitScope();
        }

        private void AnalyzeWhenStatement(WhenStatement whenStatement)
        {
            if (whenStatement.Subject != null)
                AnalyzeExpression(whenStatement.Subject);

            foreach (var entry in whenStatement.Entries)
            {
                if (entry.Conditions != null)
                {
                    foreach (var condition in entry.Conditions)
                        AnalyzeExpression(condition);
                }

                AnalyzeStatement(entry.Body);
            }
        }

        private void AnalyzeFunctionDeclaration(FunctionDeclarationStatement funcDecl)
        {
            var current = _scopes.Peek();
            if (current.ContainsKey(funcDecl.Name))
            {
                _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Semántico", $"El identificador '{funcDecl.Name}' ya fue declarado.", funcDecl.Line, funcDecl.Column));
                return;
            }

            current[funcDecl.Name] = GetDeclaredType(funcDecl.ReturnType);

            EnterScope();
            foreach (var p in funcDecl.Parameters)
            {
                _scopes.Peek()[p] = ValueTypeSymbol.Unknown;
            }

            AnalyzeStatement(funcDecl.Body);
            ExitScope();
        }

        private void AnalyzeAssignment(AssignmentStatement assignment)
        {
            var exprType = AnalyzeExpression(assignment.Value);
            if (!TryResolveSymbol(assignment.Identifier, out var existingType, out var ownerScope))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "Semántico",
                    $"La variable '{assignment.Identifier}' no está declarada.",
                    assignment.Line,
                    assignment.Column));
                return;
            }

            if (existingType == ValueTypeSymbol.Unknown)
            {
                ownerScope![assignment.Identifier] = exprType;
                return;
            }

            if (exprType != ValueTypeSymbol.Unknown && existingType != exprType)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "Semántico",
                    $"No se puede asignar '{exprType}' a variable '{assignment.Identifier}' de tipo '{existingType}'.",
                    assignment.Line,
                    assignment.Column));
            }
        }

        private void AnalyzeConditional(ExpressionNode condition, int line, int column)
        {
            var conditionType = AnalyzeExpression(condition);
            if (conditionType != ValueTypeSymbol.Bool && conditionType != ValueTypeSymbol.Unknown)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "Semántico",
                    "La condición debe evaluar a booleano.",
                    line,
                    column));
            }
        }

        private ValueTypeSymbol AnalyzeExpression(ExpressionNode expression)
        {
            switch (expression)
            {
                case LiteralExpression literal:
                    return GetLiteralType(literal.Value);

                case VariableExpression variable:
                    if (!TryResolveSymbol(variable.Name, out var type, out _))
                    {
                        _diagnostics.Add(new Diagnostic(
                            DiagnosticSeverity.Error,
                            "Semántico",
                            $"Uso de variable no declarada '{variable.Name}'.",
                            variable.Line,
                            variable.Column));
                        return ValueTypeSymbol.Unknown;
                    }
                    return type;

                case UnaryExpression unary:
                    {
                        var operandType = AnalyzeExpression(unary.Operand);
                        if (unary.Operator == "!")
                        {
                            if (operandType != ValueTypeSymbol.Bool && operandType != ValueTypeSymbol.Unknown)
                            {
                                _diagnostics.Add(new Diagnostic(
                                    DiagnosticSeverity.Error,
                                    "Semántico",
                                    "El operador '!' requiere un operando booleano.",
                                    unary.Line,
                                    unary.Column));
                            }
                            return ValueTypeSymbol.Bool;
                        }

                        if (unary.Operator == "-")
                        {
                            if (operandType != ValueTypeSymbol.Number && operandType != ValueTypeSymbol.Unknown)
                            {
                                _diagnostics.Add(new Diagnostic(
                                    DiagnosticSeverity.Error,
                                    "Semántico",
                                    "El operador '-' requiere un operando numérico.",
                                    unary.Line,
                                    unary.Column));
                            }
                            return ValueTypeSymbol.Number;
                        }

                        return ValueTypeSymbol.Unknown;
                    }

                case BinaryExpression binary:
                    return AnalyzeBinary(binary);

                case MemberAccessExpression memberAccess:
                    return AnalyzeMemberAccess(memberAccess);

                case CallExpression call:
                    return AnalyzeCall(call);

                default:
                    return ValueTypeSymbol.Unknown;
            }
        }

        private ValueTypeSymbol AnalyzeMemberAccess(MemberAccessExpression memberAccess)
        {
            if (memberAccess.Target is VariableExpression variable && IsKnownExternalIdentifier(variable.Name))
                return ValueTypeSymbol.Unknown;

            AnalyzeExpression(memberAccess.Target);
            return ValueTypeSymbol.Unknown;
        }

        private ValueTypeSymbol AnalyzeCall(CallExpression call)
        {
            if (call.Callee is VariableExpression variable && IsKnownExternalFunction(variable.Name))
            {
                foreach (var argument in call.Arguments)
                    AnalyzeExpression(argument);

                return ValueTypeSymbol.Unknown;
            }

            AnalyzeExpression(call.Callee);
            foreach (var argument in call.Arguments)
                AnalyzeExpression(argument);

            return ValueTypeSymbol.Unknown;
        }

        private ValueTypeSymbol AnalyzeBinary(BinaryExpression binary)
        {
            var left = AnalyzeExpression(binary.Left);
            var right = AnalyzeExpression(binary.Right);

            switch (binary.Operator)
            {
                case "+":
                    if (left == ValueTypeSymbol.String || right == ValueTypeSymbol.String) return ValueTypeSymbol.String;
                    if (left == ValueTypeSymbol.Number && right == ValueTypeSymbol.Number) return ValueTypeSymbol.Number;
                    break;

                case "-":
                case "*":
                case "/":
                    if (left == ValueTypeSymbol.Number && right == ValueTypeSymbol.Number) return ValueTypeSymbol.Number;
                    break;

                case "<":
                case "<=":
                case ">":
                case ">=":
                    if (left == ValueTypeSymbol.Number && right == ValueTypeSymbol.Number) return ValueTypeSymbol.Bool;
                    break;

                case "==":
                case "!=":
                    if (left != ValueTypeSymbol.Unknown && right != ValueTypeSymbol.Unknown && left != right)
                    {
                        _diagnostics.Add(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            "Semántico",
                            $"Comparación entre tipos distintos ('{left}' y '{right}').",
                            binary.Line,
                            binary.Column));
                    }
                    return ValueTypeSymbol.Bool;

                case "&&":
                case "||":
                    if (left == ValueTypeSymbol.Bool && right == ValueTypeSymbol.Bool) return ValueTypeSymbol.Bool;
                    break;
            }

            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                "Semántico",
                $"Operación '{binary.Operator}' no válida para tipos '{left}' y '{right}'.",
                binary.Line,
                binary.Column));
            return ValueTypeSymbol.Unknown;
        }

        private static ValueTypeSymbol GetLiteralType(object? value)
        {
            if (value is bool) return ValueTypeSymbol.Bool;
            if (value is string) return ValueTypeSymbol.String;
            if (value is byte or short or int or long or float or double or decimal) return ValueTypeSymbol.Number;
            return ValueTypeSymbol.Unknown;
        }

        private static ValueTypeSymbol GetDeclaredType(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return ValueTypeSymbol.Unknown;

            return typeName.Trim().TrimEnd('[', ']').ToLowerInvariant() switch
            {
                "bool" or "boolean" => ValueTypeSymbol.Bool,
                "string" => ValueTypeSymbol.String,
                "char" => ValueTypeSymbol.String,
                "void" => ValueTypeSymbol.Void,
                "byte" or "short" or "int" or "long" or "float" or "double" or "decimal" => ValueTypeSymbol.Number,
                _ => ValueTypeSymbol.Unknown
            };
        }

        private static bool IsKnownExternalIdentifier(string name)
        {
            return name.Equals("Console", StringComparison.Ordinal)
                || name.Equals("System", StringComparison.Ordinal)
                || name.Equals("Math", StringComparison.Ordinal)
                || name.Equals("String", StringComparison.Ordinal)
                || name.Equals("DateTime", StringComparison.Ordinal);
        }

        private static bool IsKnownExternalFunction(string name)
        {
            return name.Equals("println", StringComparison.Ordinal)
                || name.Equals("print", StringComparison.Ordinal)
                || name.Equals("readLine", StringComparison.Ordinal);
        }

        private void EnterScope() => _scopes.Push(new Dictionary<string, ValueTypeSymbol>());
        private void ExitScope() => _scopes.Pop();

        private bool TryResolveSymbol(string name, out ValueTypeSymbol type, out Dictionary<string, ValueTypeSymbol>? ownerScope)
        {
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out type))
                {
                    ownerScope = scope;
                    return true;
                }
            }

            type = ValueTypeSymbol.Unknown;
            ownerScope = null;
            return false;
        }
    }
}
