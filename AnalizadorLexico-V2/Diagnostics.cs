namespace AnalizadorLexico_V2
{
    // Módulo de diagnósticos: concentra errores y advertencias con origen, línea y columna.
    public enum DiagnosticSeverity
    {
        Error,
        Warning
    }

    public sealed class Diagnostic
    {
        public DiagnosticSeverity Severity { get; }
        public string Source { get; }
        public string Message { get; }
        public int Line { get; }
        public int Column { get; }

        public Diagnostic(DiagnosticSeverity severity, string source, string message, int line, int column)
        {
            Severity = severity;
            Source = source;
            Message = message;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"[{Source}] {Severity} L{Line}:C{Column} - {Message}";
        }
    }
}
