namespace AnalizadorLexico_V2
{
    // Módulo de tokens: representa cada unidad léxica detectada en código o texto.
    public enum TokenType
    {
        ID,
        NUM,
        OP,
        DEL,
        ERROR,
        KEYWORD
        ,
        STRING,
        COMMENT,
        WORD,
        PUNCT
    }

    public class TokenInfo
    {
        public TokenType Tipo { get; set; }
        public string Token { get; set; }
        public int Posicion { get; set; }
        public int Linea { get; set; }
        public int Columna { get; set; }

        public TokenInfo(TokenType tipo, string token, int posicion, int linea, int columna)
        {
            Tipo = tipo;
            Token = token;
            Posicion = posicion;
            Linea = linea;
            Columna = columna;
        }
    }
}
