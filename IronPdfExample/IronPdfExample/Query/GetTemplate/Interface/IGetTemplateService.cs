namespace IronPdfExample.Query.GetTemplate.Interface
{
    using IronPdfExample.Model;

    public interface IGetTemplateService
    {
        Task<PdfResponse> Handle(GetTemplateRequest request, CancellationToken cancellationToken);
    }
}
