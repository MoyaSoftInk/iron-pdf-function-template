namespace IronPdfExample.Query.GetTemplate.Interface
{
    using IronPdfExample.Model;

    public interface IPdfTemplateRepository
    {
        PdfTemplate GetTemplate(string templateName);
    }
}
