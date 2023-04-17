using DotLiquid;
using IronPdf;
using IronPdfExample.Converter;
using Newtonsoft.Json;
using IronPdfExample.Model;
using IronPdfExample.Repository;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using IronPdfExample.Query.GetTemplate.Interface;

namespace IronPdfExample.Query.GetTemplate;

public class GetTemplateService : IGetTemplateService
{
    private readonly IPdfTemplateRepository _templateRepository;
    private readonly TelemetryClient _telemetryClient;

    public GetTemplateService(IPdfTemplateRepository templateRepository, TelemetryClient telemetryClient)
    {
        _templateRepository = templateRepository;
        _telemetryClient = telemetryClient;
    }

    public async Task<PdfResponse> Handle(GetTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null) throw new ArgumentNullException("request can't be null");
            if (request.Template == null) throw new ArgumentNullException("request.Template can't be null");
            if (request.Data == null) throw new System.ArgumentNullException("request.Data can't be null");

            PdfTemplate template = _templateRepository.GetTemplate(request.Template);

            if (template == null)
                throw new KeyNotFoundException(string.Format("template {0} not found", request.Template));

            Pdf pdf = await GetPdf(template, request.Data, request.Password);
            return MapPdf(pdf);
        }
        catch (Exception ex)
        {

            _telemetryClient.TrackTrace($"Error: {ex.Message}", SeverityLevel.Error);
            throw ex;
        }
    }

    private PdfResponse MapPdf(Pdf pdf)
    {
        return new PdfResponse { Base64 = pdf?.Base64 };
    }


    /// <summary>
    /// Se encarga de renderizar PDF en base a la plantilla y datos proporcionados
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="datos"></param>
    /// <returns>PDF renderizado en Base64</returns>
    public async Task<Pdf> GetPdf(PdfTemplate pdfTemplate, string datos, string? password = null)
    {
        Pdf pdfResponse = new Pdf();

        try
        {
            IronPdf.Installation.LinuxAndDockerDependenciesAutoConfig = false;
            IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
            Liquid.UseRubyDateFormat = true; //Se utiliza para que liquid reconozca el formato de fechas estilo Ruby.

            if (datos == string.Empty)
            {
                string msg = "No se proporcionaron datos para generar PDF";
                throw new JsonException(msg);
            }

            if (pdfTemplate == null)
            {
                string msg = "No se proporciono un formato valido para generar PDF";
                throw new JsonException(msg);
            }

            // se deserializan los datos para poder mapearlos en la plantilla HTML
            IDictionary<string, object>? json =
                JsonConvert.DeserializeObject<IDictionary<string, object>>(datos, new DictionaryConverter());
            Hash jsonHash = Hash.FromDictionary(json);
            // Se mapean los datos en la plantilla HTML
            string renderResult = pdfTemplate.Template.Render(jsonHash);

            ChromePdfRenderer renderer = new ChromePdfRenderer(); //se define variable para trabajar con IronPdf
            renderer.RenderingOptions.FitToPaperMode =
                IronPdf.Engines.Chrome.FitToPaperModes.Automatic; //Fit HTML size on PDF
            // IBM FSM - Se ajusta el tamaño de la hoja TODO: Mejorar
            // IBM ENL - Mejorado
            if (pdfTemplate.PrintOptions != null)
            {
                renderer.RenderingOptions = pdfTemplate.PrintOptions;
            }

            PdfDocument pdfBinary = await renderer.RenderHtmlAsPdfAsync(renderResult);

            if (password != null)
            {
                pdfBinary.SecuritySettings.UserPassword = password;
            }

            pdfResponse = new Pdf
            {
                Base64 = Convert.ToBase64String(pdfBinary.BinaryData)
            };
        }
        catch (Exception ex)
        {

        }

        return pdfResponse;
    }
}