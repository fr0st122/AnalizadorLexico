namespace AnalizadorLexico_V2
{
    // Módulo de interfaz: conecta la UI con el lexer, parser, análisis semántico y análisis de texto.
    public partial class Form1 : Form
    {
        private const int TextWrapColumn = 90;
        private bool _formattingTextInput;

        public Form1()
        {
            InitializeComponent();
            SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            AplicarTema();
            lblLanguageValue.Text = ObtenerLenguajeSeleccionado();
            txtEntrada.TextChanged += txtEntrada_TextChanged;
        }

        private void AplicarTema()
        {
            BackColor = System.Drawing.Color.FromArgb(20, 22, 29);
            ForeColor = System.Drawing.Color.WhiteSmoke;

            txtEntrada.BackColor = System.Drawing.Color.FromArgb(30, 34, 45);
            txtEntrada.ForeColor = System.Drawing.Color.WhiteSmoke;
            txtEntrada.Font = new System.Drawing.Font("Consolas", 11F, FontStyle.Regular);
            txtEntrada.BorderStyle = BorderStyle.FixedSingle;

            txtResumen.BackColor = System.Drawing.Color.FromArgb(30, 34, 45);
            txtResumen.ForeColor = System.Drawing.Color.WhiteSmoke;
            txtResumen.Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Regular);

            txtDiagnosticos.BackColor = System.Drawing.Color.FromArgb(27, 31, 42);
            txtDiagnosticos.ForeColor = System.Drawing.Color.Gainsboro;
            txtDiagnosticos.Font = new System.Drawing.Font("Consolas", 10F, FontStyle.Regular);

            lblLanguageValue.ForeColor = System.Drawing.Color.FromArgb(167, 209, 255);
            lblLanguageValue.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            cmbLanguage.BackColor = System.Drawing.Color.FromArgb(30, 34, 45);
            cmbLanguage.ForeColor = System.Drawing.Color.WhiteSmoke;
            cmbLanguage.FlatStyle = FlatStyle.Flat;
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Regular);

            EstilizarBoton(btnArchivo, System.Drawing.Color.FromArgb(85, 104, 129));
            EstilizarBoton(btnAnalizar, System.Drawing.Color.FromArgb(70, 132, 246));
            EstilizarBoton(btnExport, System.Drawing.Color.FromArgb(84, 158, 107));
            dgvTokens.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTokens.AllowUserToAddRows = false;
            dgvTokens.AllowUserToDeleteRows = false;
            dgvTokens.AllowUserToResizeRows = false;
            dgvTokens.MultiSelect = false;
            dgvTokens.BackgroundColor = System.Drawing.Color.FromArgb(25, 28, 38);
            dgvTokens.BorderStyle = BorderStyle.None;
            dgvTokens.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvTokens.EnableHeadersVisualStyles = false;
            dgvTokens.GridColor = System.Drawing.Color.FromArgb(52, 57, 72);
            dgvTokens.RowHeadersVisible = false;
            dgvTokens.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(39, 45, 60);
            dgvTokens.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.WhiteSmoke;
            dgvTokens.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            dgvTokens.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Regular);
            dgvTokens.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(58, 84, 126);
            dgvTokens.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            dgvTokens.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(31, 35, 46);
            dgvTokens.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTokens.RowTemplate.Height = 28;
        }

        private static void EstilizarBoton(Button button, System.Drawing.Color color)
        {
            button.BackColor = color;
            button.ForeColor = System.Drawing.Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        }

        private void dgvTokens_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = sender as DataGridView;
            if (grid?.Rows[e.RowIndex].DataBoundItem is TokenInfo info)
            {
                var row = grid.Rows[e.RowIndex];
                switch (info.Tipo)
                {
                    case TokenType.KEYWORD:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(75, 156, 245);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(42, 116, 196);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.ID:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Gainsboro;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(70, 70, 74);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.NUM:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(59, 120, 90);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(42, 95, 70);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.OP:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(147, 116, 52);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(120, 90, 32);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.DEL:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(93, 93, 93);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(70, 70, 70);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.ERROR:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(186, 66, 66);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(145, 35, 35);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.STRING:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(162, 85, 124);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(128, 60, 95);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.COMMENT:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(83, 101, 121);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(65, 80, 96);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                    case TokenType.WORD:
                    case TokenType.PUNCT:
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(111, 78, 128);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(88, 58, 102);
                        row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                        break;
                }
            }
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvTokens.DataSource is not System.Collections.IEnumerable list)
                {
                    MessageBox.Show("No hay tokens para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|JSON files (*.json)|*.json",
                    Title = "Exportar tokens",
                    FileName = "tokens"
                };

                if (sfd.ShowDialog() != DialogResult.OK) return;

                if (sfd.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    ExportCsv(sfd.FileName, list);
                else
                    ExportJson(sfd.FileName, list);

                MessageBox.Show("Exportación completada.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCsv(string path, System.Collections.IEnumerable items)
        {
            using var sw = new System.IO.StreamWriter(path, false, System.Text.Encoding.UTF8);
            sw.WriteLine("Tipo,Token,Posicion,Linea,Columna");
            foreach (var obj in items)
            {
                if (obj is TokenInfo t)
                {
                    var line = $"{t.Tipo},{EscapeCsv(t.Token)},{t.Posicion},{t.Linea},{t.Columna}";
                    sw.WriteLine(line);
                }
            }
        }

        private string EscapeCsv(string s)
        {
            if (s == null) return "";
            if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
                return '"' + s.Replace("\"", "\"\"") + '"';
            return s;
        }

        private void ExportJson(string path, System.Collections.IEnumerable items)
        {
            var list = new System.Collections.Generic.List<TokenInfo>();
            foreach (var obj in items)
            {
                if (obj is TokenInfo t) list.Add(t);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }

        private void btnAnalizar_Click(object? sender, EventArgs e)
        {
            try
            {
                string input = txtEntrada.Text ?? string.Empty;
                bool textModeSelected = IsTextModeSelected();
                var language = textModeSelected ? Language.CSharp : GetSelectedLanguageEnum();
                var lexer = new Lexer(language);
                var mode = textModeSelected ? InputMode.Texto : lexer.DetectarModo(input);
                var tokens = lexer.Analizar(input, mode);

                dgvTokens.DataSource = null;
                dgvTokens.DataSource = tokens;
                ConfigurarEncabezadosGrid();

                var diagnostics = new List<Diagnostic>();
                foreach (var token in tokens.Where(t => t.Tipo == TokenType.ERROR))
                {
                    diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, "Léxico", $"Token inválido '{token.Token}'.", token.Linea, token.Columna));
                }

                ParseResult? parseResult = null;
                if (mode == InputMode.Texto)
                {
                    diagnostics.AddRange(new TextAnalysisService().Analyze(input, tokens));
                }
                else
                {
                    parseResult = new Parser(tokens, language).ParseProgram();
                    diagnostics.AddRange(parseResult.Diagnostics);

                    if (parseResult.Success)
                    {
                        diagnostics.AddRange(new SemanticAnalyzer().Analyze(parseResult.Program));
                    }
                }

                string selectedLanguage = ObtenerLenguajeSeleccionado();
                var insights = new CodeInsightsService().BuildInsights(input, mode, tokens, parseResult, selectedLanguage);
                lblLanguageValue.Text = insights.Language;
                txtResumen.Text = insights.Summary;
                MostrarDiagnosticos(mode, diagnostics, tokens.Count, input);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al analizar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerLenguajeSeleccionado()
        {
            string? selected = cmbLanguage.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selected))
                return "C#";

            return selected;
        }

        private Language GetSelectedLanguageEnum()
        {
            var sel = cmbLanguage.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(sel)) return Language.CSharp;

            return sel.Trim().ToLowerInvariant() switch
            {
                "kotlin" => Language.Kotlin,
                "java" => Language.Java,
                "c#" => Language.CSharp,
                "csharp" => Language.CSharp,
                _ => Language.CSharp
            };
        }

        private bool IsTextModeSelected()
        {
            return string.Equals(cmbLanguage.SelectedItem?.ToString(), "Texto", StringComparison.OrdinalIgnoreCase);
        }

        private void cmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
        {
            lblLanguageValue.Text = ObtenerLenguajeSeleccionado();
            FormatTextInputIfNeeded();
        }

        private void txtEntrada_TextChanged(object? sender, EventArgs e)
        {
            FormatTextInputIfNeeded();
        }

        private void FormatTextInputIfNeeded()
        {
            if (_formattingTextInput || !IsTextModeSelected())
                return;

            string current = txtEntrada.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(current))
                return;

            if (!NeedsTextWrapping(current, TextWrapColumn))
                return;

            string formatted = WrapPlainText(current, TextWrapColumn);
            if (formatted == current)
                return;

            int selectionStart = Math.Min(txtEntrada.SelectionStart, formatted.Length);
            _formattingTextInput = true;
            try
            {
                txtEntrada.Text = formatted;
                txtEntrada.SelectionStart = selectionStart;
                txtEntrada.SelectionLength = 0;
            }
            finally
            {
                _formattingTextInput = false;
            }
        }

        private static bool NeedsTextWrapping(string text, int maxLineLength)
        {
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            return normalized.Split('\n').Any(line => line.Length > maxLineLength);
        }

        private static string WrapPlainText(string text, int maxLineLength)
        {
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
            if (normalized.Length == 0)
                return string.Empty;

            var paragraphs = System.Text.RegularExpressions.Regex.Split(normalized, @"\n\s*\n");
            var result = new System.Text.StringBuilder();

            foreach (string paragraph in paragraphs)
            {
                string cleanParagraph = System.Text.RegularExpressions.Regex.Replace(paragraph, @"\s+", " ").Trim();
                if (cleanParagraph.Length == 0)
                    continue;

                if (result.Length > 0)
                    result.AppendLine().AppendLine();

                AppendWrappedParagraph(result, cleanParagraph, maxLineLength);
            }

            return result.ToString();
        }

        private static void AppendWrappedParagraph(System.Text.StringBuilder result, string paragraph, int maxLineLength)
        {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int currentLineLength = 0;

            foreach (string word in words)
            {
                if (currentLineLength == 0)
                {
                    result.Append(word);
                    currentLineLength = word.Length;
                    continue;
                }

                if (currentLineLength + 1 + word.Length > maxLineLength)
                {
                    result.AppendLine();
                    result.Append(word);
                    currentLineLength = word.Length;
                    continue;
                }

                result.Append(' ').Append(word);
                currentLineLength += 1 + word.Length;
            }
        }

        private void ConfigurarEncabezadosGrid()
        {
            if (dgvTokens.Columns.Count == 0) return;
            if (dgvTokens.Columns.Contains("Tipo") && dgvTokens.Columns["Tipo"] is DataGridViewColumn tipoCol) tipoCol.HeaderText = "Tipo";
            if (dgvTokens.Columns.Contains("Token") && dgvTokens.Columns["Token"] is DataGridViewColumn tokenCol) tokenCol.HeaderText = "Token";
            if (dgvTokens.Columns.Contains("Posicion") && dgvTokens.Columns["Posicion"] is DataGridViewColumn posicionCol) posicionCol.HeaderText = "Posición";
            if (dgvTokens.Columns.Contains("Linea") && dgvTokens.Columns["Linea"] is DataGridViewColumn lineaCol) lineaCol.HeaderText = "Línea";
            if (dgvTokens.Columns.Contains("Columna") && dgvTokens.Columns["Columna"] is DataGridViewColumn columnaCol) columnaCol.HeaderText = "Columna";
        }

        private void MostrarDiagnosticos(InputMode mode, List<Diagnostic> diagnostics, int tokenCount, string input)
        {
            int errors = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
            int warnings = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
            string estado = errors == 0 ? "OK" : "CON ERRORES";
            string nombreModo = mode == InputMode.Codigo ? "Código" : "Texto";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Modo detectado: {nombreModo}");
            sb.AppendLine($"Tokens: {tokenCount}");
            sb.AppendLine($"Errores: {errors} | Warnings: {warnings}");
            sb.AppendLine($"Estado: {estado}");
            sb.AppendLine();

            if (diagnostics.Count == 0)
            {
                sb.AppendLine("Sin diagnósticos.");
            }
            else
            {
                foreach (var d in diagnostics.OrderBy(d => d.Line).ThenBy(d => d.Column))
                {
                    sb.AppendLine(d.ToString());
                    string location = BuildDiagnosticLocation(input, d.Line, d.Column);
                    if (!string.IsNullOrWhiteSpace(location))
                        sb.AppendLine(location);
                }
            }

            txtDiagnosticos.Text = sb.ToString();
        }

        private static string BuildDiagnosticLocation(string input, int line, int column)
        {
            if (string.IsNullOrEmpty(input) || line <= 0)
                return string.Empty;

            var lines = input.Replace("\r\n", "\n").Split('\n');
            if (line > lines.Length)
                return string.Empty;

            string sourceLine = lines[line - 1].Replace("\t", "    ");
            if (sourceLine.Length == 0)
                return string.Empty;

            int markerColumn = Math.Clamp(column, 1, sourceLine.Length);
            const int contextRadius = 60;
            int start = Math.Max(0, markerColumn - contextRadius - 1);
            int end = Math.Min(sourceLine.Length, markerColumn + contextRadius - 1);
            string prefix = start > 0 ? "... " : string.Empty;
            string suffix = end < sourceLine.Length ? " ..." : string.Empty;
            string excerpt = prefix + sourceLine[start..end] + suffix;
            int caretColumn = prefix.Length + markerColumn - start - 1;

            return excerpt + Environment.NewLine + new string(' ', Math.Max(0, caretColumn)) + "^";
        }

        private async void btnArchivo_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Word files (*.docx)|*.docx",
                Title = "Seleccionar archivo"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var reader = new FileReader();
                    string contenido = string.Empty;
                    Cursor previous = Cursor.Current ?? Cursors.Default;
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        if (ofd.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                            contenido = await reader.LeerTxtAsync(ofd.FileName).ConfigureAwait(false);
                        else if (ofd.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                            contenido = await reader.LeerDocxAsync(ofd.FileName).ConfigureAwait(false);
                    }
                    finally
                    {
                        Cursor.Current = previous;
                    }

                    if (txtEntrada.InvokeRequired)
                        txtEntrada.Invoke(() => txtEntrada.Text = contenido);
                    else
                        txtEntrada.Text = contenido;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al leer archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
