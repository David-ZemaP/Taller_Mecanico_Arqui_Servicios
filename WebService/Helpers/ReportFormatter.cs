namespace Taller_Mecanico_WebService.Helpers;

/// <summary>
/// Helper para formatear estilos visuales en reportes PDF y Excel
/// Define colores, fuentes y estructura corporativa del Taller Mecánico
/// </summary>
public class ReportFormatter
{
    private readonly ILogger<ReportFormatter> _logger;

    // Colores corporativos del Taller Mecánico
    public static class Colors
    {
        public const string PrimaryRGB = "41, 128, 185";      // Azul corporativo
        public const string SecondaryRGB = "230, 126, 34";    // Naranja acento
        public const string BackgroundRGB = "236, 240, 241";  // Gris claro
        public const string TextRGB = "44, 62, 80";           // Gris oscuro
        public const string BorderRGB = "189, 195, 199";      // Gris bordes
        public const string HighlightRGB = "142, 68, 173";    // Púrpura para totales

        // Valores RGB como byte arrays (para iTextSharp)
        public static readonly byte[] Primary = { 41, 128, 185 };
        public static readonly byte[] Secondary = { 230, 126, 34 };
        public static readonly byte[] Background = { 236, 240, 241 };
        public static readonly byte[] Text = { 44, 62, 80 };
        public static readonly byte[] Border = { 189, 195, 199 };
        public static readonly byte[] Highlight = { 142, 68, 173 };
    }

    public static class FontSizes
    {
        public const int TitleSize = 24;
        public const int SubtitleSize = 14;
        public const int HeaderSize = 12;
        public const int BodySize = 10;
        public const int FooterSize = 9;
    }

    public ReportFormatter(ILogger<ReportFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene la configuración de espaciado para PDF
    /// </summary>
    public Dictionary<string, float> GetMarginConfig()
    {
        return new Dictionary<string, float>
        {
            { "top", 40f },
            { "bottom", 50f },
            { "left", 30f },
            { "right", 30f }
        };
    }

    /// <summary>
    /// Formatea moneda boliviana para reportes
    /// </summary>
    public string FormatMoneda(decimal valor)
    {
        return $"Bs. {valor:N2}";
    }

    /// <summary>
    /// Formatea fecha en formato local
    /// </summary>
    public string FormatFecha(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy");
    }

    /// <summary>
    /// Formatea fecha y hora completa
    /// </summary>
    public string FormatFechaHora(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy HH:mm:ss");
    }

    /// <summary>
    /// Obtiene el encabezado formateado para reportes
    /// Formato: [Logo] | TÍTULO REPORTE
    /// </summary>
    public string GetEncabezado(string tituloReporte)
    {
        return $"[ LOGO TALLER MECÁNICO ] | {tituloReporte}";
    }

    /// <summary>
    /// Genera línea de separación visual
    /// </summary>
    public string GetSeparador()
    {
        return "═══════════════════════════════════════════════════════════════";
    }

    /// <summary>
    /// Obtiene configuración de hoja Excel
    /// </summary>
    public Dictionary<string, object> GetExcelStyleConfig()
    {
        return new Dictionary<string, object>
        {
            { "fontName", "Calibri" },
            { "fontSize", 11 },
            { "alignment", "Center" },
            { "headerBgColor", "2980B9" },  // Azul corporativo en hex
            { "headerFontColor", "FFFFFF" }, // Blanco
            { "alternateRowColor", "ECF0F1" } // Gris claro para filas alternas
        };
    }

    /// <summary>
    /// Formatea un número con separadores de miles
    /// </summary>
    public string FormatNumero(decimal numero)
    {
        return numero.ToString("N2");
    }

    /// <summary>
    /// Formatea un número entero con separadores de miles
    /// </summary>
    public string FormatNumeroEntero(int numero)
    {
        return numero.ToString("N0");
    }

    /// <summary>
    /// Obtiene el pie de página estándar para reportes
    /// </summary>
    public string GetPieReporte(string infoAuditoria, int pagina = 1, int totalPaginas = 1)
    {
        return $"Página {pagina} de {totalPaginas}\n{infoAuditoria}";
    }

    /// <summary>
    /// Valida y trunca nombres largos para tabla
    /// </summary>
    public string TruncateText(string texto, int maxLength = 30)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        if (texto.Length <= maxLength)
            return texto;

        return texto.Substring(0, maxLength) + "...";
    }
}
