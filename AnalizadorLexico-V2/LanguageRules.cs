using System.Collections.Generic;

namespace AnalizadorLexico_V2
{
    // Módulo de reglas por lenguaje: centraliza operadores, delimitadores y terminadores.
    internal static class LanguageRules
    {
        public static IReadOnlyList<string> GetOperators(Language lang)
        {
            var list = lang switch
            {
                Language.Kotlin => new List<string> {"===","==","!=","<=",">=","&&","||","++","--","+=","-=","*=","/=","->","::","?:","..",".","+","-","*","/","=","<",">","!","&","|","^","%"},
                Language.CSharp => new List<string> {"===","==","!=","<=",">=","&&","||","<<",">>","++","--","+=","-=","*=","/=","%=","::","?:","?.","->",".","+","-","*","/","=","<",">","!","&","|","^","%","~"},
                Language.Java => new List<string> {"==","!=","<=",">=","&&","||","++","--","+=","-=","*=","/=","%=","<<",">>",">>>",".","+","-","*","/","=","<",">","!","&","|","^","%"},
                _ => new List<string> {"==","!=","<=",">=","&&","||",".","+","-","*","/","=","<",">","!","&","|"}
            };

            list.Sort((a, b) => b.Length.CompareTo(a.Length));
            return list;
        }

        public static string GetDelimiters(Language lang)
        {
            return lang switch
            {
                Language.Kotlin => ";,(){}[]:.",
                Language.CSharp => ";,(){}[]:.",
                Language.Java => ";,(){}[]:.",
                _ => ";,(){}[]:."
            };
        }

        public static bool SemicolonRequired(Language lang)
        {
            return lang == Language.CSharp || lang == Language.Java;
        }
    }
}
