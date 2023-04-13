using DotLiquid;

namespace IronPdfExample.Model
{

    public class PdfTemplate
    {
        public string TemplatePath { get; set; }
        public string PrintOptionsPath { get; set; }
        public string TemplateName { get; set; }
        public Template Template { get; set; }
        public ChromePdfRenderOptions PrintOptions { get; set;}
    }
}
