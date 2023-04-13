using IronPdfExample.Configurations;
using IronPdfExample.Query.GetTemplate;
using IronPdfExample.Repository;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.UseNewtonsoftJson();
    })
    .ConfigureOpenApi()
    .ConfigureServices((_, services) =>
    {
        var rootConf = new RootConfiguration("Assets/");
        services.AddSingleton(rootConf);
        var templateRepository = new PdfTemplateRepository(rootConf);
        services.AddSingleton(templateRepository);
        services.AddSingleton(new GetTemplateService(templateRepository));

    })
    .Build();

host.Run();
