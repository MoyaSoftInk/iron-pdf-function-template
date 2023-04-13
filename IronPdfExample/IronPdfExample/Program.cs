using IronPdfExample.Configurations;
using IronPdfExample.Query.GetTemplate;
using IronPdfExample.Query.GetTemplate.Interface;
using IronPdfExample.Repository;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.UseNewtonsoftJson();
    })
    .ConfigureOpenApi()
    .ConfigureServices((_, services ) =>
    {
        var rootConf = new RootConfiguration("Assets/");

        ApplicationInsightsServiceOptions aiOptions = new()
        {
            EnableAdaptiveSampling = false,
        };

        services.AddApplicationInsightsTelemetryWorkerService(aiOptions);

        services.AddSingleton(rootConf);

        services.AddSingleton<IPdfTemplateRepository, PdfTemplateRepository>();
        services.AddSingleton<IGetTemplateService, GetTemplateService>();

    })
    .Build();

host.Run();
