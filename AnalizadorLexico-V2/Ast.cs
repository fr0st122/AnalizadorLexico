using System;
using System.Collections.Generic;

namespace AnalizadorLexico_V2
{
    // Módulo de AST: define la estructura intermedia que produce el parser y consume el analizador semántico.
    public sealed class ProgramNode
    {
        public List<StatementNode> Statements { get; } = new();
    }

    public abstract class SyntaxNode
    {
        public int Line { get; }
        public int Column { get; }

        protected SyntaxNode(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }

    public abstract class StatementNode : SyntaxNode
    {
        protected StatementNode(int line, int column) : base(line, column) { }
    }

    public abstract class ExpressionNode : SyntaxNode
    {
        protected ExpressionNode(int line, int column) : base(line, column) { }
    }

    public sealed class BlockStatement : StatementNode
    {
        public List<StatementNode> Statements { get; } = new();
        public BlockStatement(int line, int column) : base(line, column) { }
    }

    public sealed class VarDeclarationStatement : StatementNode
    {
        public string Identifier { get; }
        public string? DeclaredType { get; }
        public ExpressionNode? Initializer { get; }
        public bool IsMutable { get; }

        public VarDeclarationStatement(string identifier, ExpressionNode? initializer, bool isMutable, int line, int column)
            : this(identifier, null, initializer, isMutable, line, column)
        {
        }

        public VarDeclarationStatement(string identifier, string? declaredType, ExpressionNode? initializer, bool isMutable, int line, int column) : base(line, column)
        {
            Identifier = identifier;
            DeclaredType = declaredType;
            Initializer = initializer;
            IsMutable = isMutable;
        }
    }

    public sealed class ClassDeclarationStatement : StatementNode
    {
        public string Name { get; }
        public List<string> Modifiers { get; } = new();
        public List<StatementNode> Members { get; } = new();

        public ClassDeclarationStatement(string name, IEnumerable<string> modifiers, IEnumerable<StatementNode> members, int line, int column) : base(line, column)
        {
            Name = name;
            Modifiers.AddRange(modifiers);
            Members.AddRange(members);
        }
    }

    public sealed class DirectiveStatement : StatementNode
    {
        public string Kind { get; }
        public string Name { get; }

        public DirectiveStatement(string kind, string name, int line, int column) : base(line, column)
        {
            Kind = kind;
            Name = name;
        }
    }

    public sealed class NamespaceDeclarationStatement : StatementNode
    {
        public string Name { get; }
        public List<StatementNode> Members { get; } = new();

        public NamespaceDeclarationStatement(string name, IEnumerable<StatementNode> members, int line, int column) : base(line, column)
        {
            Name = name;
            Members.AddRange(members);
        }
    }

    public sealed class FunctionDeclarationStatement : StatementNode
    {
        public string Name { get; }
        public string? ReturnType { get; }
        public List<string> Modifiers { get; } = new();
        public List<string> Parameters { get; } = new();
        public BlockStatement Body { get; }

        public FunctionDeclarationStatement(string name, IEnumerable<string> parameters, BlockStatement body, int line, int column)
            : this(name, null, Array.Empty<string>(), parameters, body, line, column)
        {
        }

        public FunctionDeclarationStatement(string name, string? returnType, IEnumerable<string> modifiers, IEnumerable<string> parameters, BlockStatement body, int line, int column) : base(line, column)
        {
            Name = name;
            ReturnType = returnType;
            Modifiers.AddRange(modifiers);
            Parameters.AddRange(parameters);
            Body = body;
        }
    }

    public sealed class WhenEntry
    {
        public List<ExpressionNode>? Conditions { get; }
        public StatementNode Body { get; }

        public WhenEntry(List<ExpressionNode>? conditions, StatementNode body)
        {
            Conditions = conditions;
            Body = body;
        }
    }

    public sealed class WhenStatement : StatementNode
    {
        public ExpressionNode? Subject { get; }
        public List<WhenEntry> Entries { get; } = new();

        public WhenStatement(ExpressionNode? subject, int line, int column) : base(line, column)
        {
            Subject = subject;
        }
    }

    public sealed class ForStatement : StatementNode
    {
        public string? Iterator { get; }
        public ExpressionNode? Iterable { get; }
        public StatementNode Body { get; }

        public ForStatement(string? iterator, ExpressionNode? iterable, StatementNode body, int line, int column) : base(line, column)
        {
            Iterator = iterator;
            Iterable = iterable;
            Body = body;
        }
    }

    public sealed class AssignmentStatement : StatementNode
    {
        public string Identifier { get; }
        public ExpressionNode Value { get; }

        public AssignmentStatement(string identifier, ExpressionNode value, int line, int column) : base(line, column)
        {
            Identifier = identifier;
            Value = value;
        }
    }

    public sealed class ExpressionStatement : StatementNode
    {
        public ExpressionNode Expression { get; }
        public ExpressionStatement(ExpressionNode expression, int line, int column) : base(line, column)
        {
            Expression = expression;
        }
    }

    public sealed class IfStatement : StatementNode
    {
        public ExpressionNode Condition { get; }
        public StatementNode ThenBranch { get; }
        public StatementNode? ElseBranch { get; }

        public IfStatement(ExpressionNode condition, StatementNode thenBranch, StatementNode? elseBranch, int line, int column) : base(line, column)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }
    }

    public sealed class WhileStatement : StatementNode
    {
        public ExpressionNode Condition { get; }
        public StatementNode Body { get; }

        public WhileStatement(ExpressionNode condition, StatementNode body, int line, int column) : base(line, column)
        {
            Condition = condition;
            Body = body;
        }
    }

    public sealed class ReturnStatement : StatementNode
    {
        public ExpressionNode? Expression { get; }
        public ReturnStatement(ExpressionNode? expression, int line, int column) : base(line, column)
        {
            Expression = expression;
        }
    }

    public sealed class LiteralExpression : ExpressionNode
    {
        public object? Value { get; }
        public LiteralExpression(object? value, int line, int column) : base(line, column)
        {
            Value = value;
        }
    }

    public sealed class VariableExpression : ExpressionNode
    {
        public string Name { get; }
        public VariableExpression(string name, int line, int column) : base(line, column)
        {
            Name = name;
        }
    }

    public sealed class MemberAccessExpression : ExpressionNode
    {
        public ExpressionNode Target { get; }
        public string MemberName { get; }

        public MemberAccessExpression(ExpressionNode target, string memberName, int line, int column) : base(line, column)
        {
            Target = target;
            MemberName = memberName;
        }
    }

    public sealed class CallExpression : ExpressionNode
    {
        public ExpressionNode Callee { get; }
        public List<ExpressionNode> Arguments { get; } = new();

        public CallExpression(ExpressionNode callee, IEnumerable<ExpressionNode> arguments, int line, int column) : base(line, column)
        {
            Callee = callee;
            Arguments.AddRange(arguments);
        }
    }

    public sealed class UnaryExpression : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Operand { get; }

        public UnaryExpression(string @operator, ExpressionNode operand, int line, int column) : base(line, column)
        {
            Operator = @operator;
            Operand = operand;
        }
    }

    public sealed class BinaryExpression : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }

        public BinaryExpression(string @operator, ExpressionNode left, ExpressionNode right, int line, int column) : base(line, column)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }
    }
}
