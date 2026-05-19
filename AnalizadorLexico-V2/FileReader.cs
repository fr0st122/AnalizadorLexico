using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

#if USE_OPENXML
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
#endif

namespace AnalizadorLexico_V2
{
    // Módulo de lectura: carga contenido desde archivos .txt y .docx para enviarlo al analizador.
    public class FileReader
    {
        public string LeerTxt(string ruta)
        {
            return File.ReadAllText(ruta, Encoding.UTF8);
        }

        public async Task<string> LeerTxtAsync(string ruta)
        {
            return await File.ReadAllTextAsync(ruta, Encoding.UTF8).ConfigureAwait(false);
        }

#if USE_OPENXML
        public string LeerDocx(string ruta)
        {
            var sb = new StringBuilder();

            using (WordprocessingDocument doc = WordprocessingDocument.Open(ruta, false))
            {
                var mainPart = doc.MainDocumentPart;
                if (mainPart == null || mainPart.Document?.Body == null) return string.Empty;
                var body = mainPart.Document.Body;

                // Incluye párrafos del cuerpo y de tablas en el orden del documento.
                var paragraphs = body.Descendants<Paragraph>();
                foreach (var para in paragraphs)
                {
                    var text = string.Concat(para.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text));
                    sb.AppendLine(text);
                }

                if (mainPart.HeaderParts != null)
                {
                    foreach (var header in mainPart.HeaderParts)
                    {
                        var headerRoot = header.Header;
                        if (headerRoot == null) continue;
                        foreach (var para in headerRoot.Descendants<Paragraph>())
                        {
                            var text = string.Concat(para.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text));
                            sb.AppendLine(text);
                        }
                    }
                }

                if (mainPart.FooterParts != null)
                {
                    foreach (var footer in mainPart.FooterParts)
                    {
                        var footerRoot = footer.Footer;
                        if (footerRoot == null) continue;
                        foreach (var para in footerRoot.Descendants<Paragraph>())
                        {
                            var text = string.Concat(para.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text));
                            sb.AppendLine(text);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public Task<string> LeerDocxAsync(string ruta)
        {
            return Task.Run(() => LeerDocx(ruta));
        }
#else
        public string LeerDocx(string ruta)
        {
            return "[Lectura de .docx no disponible. Instale DocumentFormat.OpenXml y defina USE_OPENXML para habilitarlo.]";
        }

        public Task<string> LeerDocxAsync(string ruta)
        {
            return Task.FromResult(LeerDocx(ruta));
        }
#endif
    }
}
