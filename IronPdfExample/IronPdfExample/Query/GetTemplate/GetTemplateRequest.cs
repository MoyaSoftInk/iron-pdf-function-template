namespace IronPdfExample.Query.GetTemplate;

public class GetTemplateRequest
{
    /// <summary>
    /// datos para el pdf
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// nombre de la plantilla a utilizar
    /// </summary>
    public string? Template { get; set; }
    /// <summary>
    /// contraseña de la plantilla a utilizar
    /// </summary>
    public string? Password { get; set; }
}