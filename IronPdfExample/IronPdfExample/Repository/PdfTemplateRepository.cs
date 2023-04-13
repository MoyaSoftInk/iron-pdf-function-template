using System.Collections.Concurrent;
using DotLiquid;
using IronPdfExample.Configurations;
using IronPdfExample.Converter;
using IronPdfExample.Model;
using IronPdfExample.Query.GetTemplate.Interface;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace IronPdfExample.Repository;

public sealed class PdfTemplateRepository : IPdfTemplateRepository
{
    private readonly static ConcurrentDictionary<string, PdfTemplate> templateCache = new ConcurrentDictionary<string, PdfTemplate>();

   
    private readonly string _root;
    private readonly TelemetryClient _telemetryClient;

    public PdfTemplateRepository(RootConfiguration root, TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _root = root.Root;
    }

    /// <summary>
    /// Se encarga de proprocinar un template parseado en base a un identificador
    /// </summary>
    /// <param name="templateName"></param>
    /// <returns>PDF Template from FS</returns>
    public PdfTemplate GetTemplate(string templateName)
    {
        _telemetryClient.TrackTrace($"SOLICITA TEMPLATE {templateName}", SeverityLevel.Information);

        if (templateName == string.Empty)
        {
            string msg = String.Format("TemplateName no informado", templateName);
            throw new KeyNotFoundException(msg);
        }

        if (templateCache.ContainsKey(templateName))
        {
            return templateCache[templateName];
        }

        PdfTemplate pdfTemplate = new PdfTemplate
        {
            TemplateName = templateName,
            TemplatePath = _root + templateName + ".html",
            PrintOptionsPath = _root + templateName + ".json"
        };

        StreamReader streamReader = new StreamReader(pdfTemplate.TemplatePath);
        try
        {
            string stemplate = streamReader.ReadToEnd();
            stemplate = stemplate.Replace("##ROOT##", _root);
            pdfTemplate.Template = Template.Parse(stemplate);
        }
        catch (IOException ex)
        {
            string msg = String.Format("Template #{0} no cargado: {1}", templateName, ex.Message);
            throw new KeyNotFoundException(msg);
        }
        finally
        {
            streamReader.Close();
        }

        streamReader = new StreamReader(pdfTemplate.PrintOptionsPath);
        try
        {
            string datos = streamReader.ReadToEnd();
            datos = datos.Replace("##ROOT##", _root);
            IDictionary<string, object> json = JsonConvert.DeserializeObject<IDictionary<string, object>>(datos, new DictionaryConverter());
            if (json != null && json.ContainsKey("PrintOptions"))
            {
                IDictionary<string, object> options = (IDictionary<string, object>)json["PrintOptions"];
                pdfTemplate.PrintOptions = new ChromePdfRenderOptions
                {
                    PaperSize = GetPdfPaperSize(options["PaperSize"]),
                    MarginTop = GetDouble(options["MarginTop"]),
                    MarginLeft = GetDouble(options["MarginLeft"]),
                    MarginRight = GetDouble(options["MarginRight"]),
                    MarginBottom = GetDouble(options["MarginBottom"]),
                    CssMediaType = GetPdfCssMediaType(options["CssMediaType"]),
                    EnableJavaScript = GetBool(options["EnableJavaScript"]),
                    UseMarginsOnHeaderAndFooter = UseMargins.LeftAndRight
                };
                if (options.ContainsKey("Header.HtmlFragment"))
                {
                    pdfTemplate.PrintOptions.HtmlHeader = new HtmlHeaderFooter()
                    {
                        HtmlFragment = GetString(options["Header.HtmlFragment"])
                    };
                }
                pdfTemplate.PrintOptions.TextFooter.CenterText = GetString(options["Footer.CenterText"]);
                pdfTemplate.PrintOptions.TextFooter.FontSize = GetInt(options["Footer.FontSize"]);
                pdfTemplate.PrintOptions.TextFooter.Font = IronPdf.Font.FontTypes.Arial;
            }

        }
        catch (IOException ex)
        {
            _telemetryClient.TrackTrace($"Template #{templateName} sin print options: {ex.Message}", SeverityLevel.Error);
            Exception exn = new Exception($"Template #{templateName} sin print options: {ex.Message}", ex);
            throw exn;
        }
        finally
        {
            streamReader.Close();
        }
        templateCache[templateName] = pdfTemplate;
        return pdfTemplate;
    }
    private IronPdf.Rendering.PdfCssMediaType GetPdfCssMediaType(object config)
    {
        if (!(config is string))
            return IronPdf.Rendering.PdfCssMediaType.Print;
        string sconfig = (string)config;
        if (sconfig.Equals("PdfPrintOptions.PdfCssMediaType.Print"))
            return IronPdf.Rendering.PdfCssMediaType.Print;
        if (sconfig.Equals("PdfPrintOptions.PdfCssMediaType.Screen"))
            return IronPdf.Rendering.PdfCssMediaType.Screen;
        return IronPdf.Rendering.PdfCssMediaType.Print;
    }
    private double GetDouble(object config)
    {
        if (config is double @double)
            return @double;
        try
        {
            return double.Parse("" + config);
        }
        catch
        {
            return 0;
        }
    }
    private int GetInt(object config)
    {
        if (config is int @int)
            return @int;
        try
        {
            return int.Parse("" + config);
        }
        catch
        {
            return 0;
        }
    }
    private bool GetBool(object config)
    {
        if (config is bool boolean)
            return boolean;
        try
        {
            return bool.Parse("" + config);
        }
        catch
        {
            return false;
        }
    }
    private string GetString(object config)
    {
        if (config is string @string)
            return @string;
        return config?.ToString();
    }

    private IronPdf.Rendering.PdfPaperSize GetPdfPaperSize(object config)
    {
        if (!(config is string))
            return IronPdf.Rendering.PdfPaperSize.Letter;
        string sconfig = (string)config;
        if (sconfig.Equals("PdfPrintOptions.PdfPaperSize.Letter"))
            return IronPdf.Rendering.PdfPaperSize.Letter;
        if (sconfig.Equals("PdfPrintOptions.PdfPaperSize.Legal"))
            return IronPdf.Rendering.PdfPaperSize.Legal;
        if (sconfig.Equals("PdfPrintOptions.PdfPaperSize.A4"))
            return IronPdf.Rendering.PdfPaperSize.A4;
        return IronPdf.Rendering.PdfPaperSize.Letter;
    }
}