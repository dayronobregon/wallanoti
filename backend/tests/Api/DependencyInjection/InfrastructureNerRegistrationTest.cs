using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallanoti.Api.Extension.DependencyInjection;
using Wallanoti.Src.Alerts.Domain.Services;
using Wallanoti.Src.Alerts.Infrastructure.Services;

namespace Wallanoti.Tests.Api.DependencyInjection;

public class InfrastructureNerRegistrationTest
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? nerConfig = null)
    {
        var configDict = new Dictionary<string, string?>
        {
            ["RabbitMq:HostName"] = "localhost",
            ["RabbitMq:UserName"] = "guest",
            ["RabbitMq:Password"] = "guest",
            ["RabbitMq:VirtualHost"] = "/",
            ["RabbitMq:Port"] = "5672"
        };

        if (nerConfig != null)
        {
            foreach (var kvp in nerConfig)
            {
                configDict[kvp.Key] = kvp.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    }

    [Fact]
    public void AddInfrastructure_NerServiceSection_BindsNerServiceOptions()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["NerService:Provider"] = "HuggingFace",
            ["NerService:TimeoutSeconds"] = "45",
            ["NerService:ModelUrl"] = "https://api-inference.huggingface.co/models/dslim/bert-base-NER",
            ["NerService:ApiKey"] = "test-api-key"
        });

        var services = new ServiceCollection();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<NerServiceOptions>();
        Assert.Equal("HuggingFace", options.Provider);
        Assert.Equal(45, options.TimeoutSeconds);
        Assert.Equal("https://api-inference.huggingface.co/models/dslim/bert-base-NER", options.ModelUrl);
        Assert.Equal("test-api-key", options.ApiKey);
    }

    [Fact]
    public void AddInfrastructure_NerServices_RegistersINerService()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["NerService:Provider"] = "HuggingFace",
            ["NerService:TimeoutSeconds"] = "30",
            ["NerService:ModelUrl"] = "https://api-inference.huggingface.co/models/dslim/bert-base-NER",
            ["NerService:ApiKey"] = "test-api-key"
        });

        var services = new ServiceCollection();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var nerService = serviceProvider.GetRequiredService<INerService>();
        Assert.NotNull(nerService);
    }

    [Fact]
    public void AddInfrastructure_NerServices_RegistersIWallapopUrlBuilder()
    {
        // Arrange - provide minimal NerService config
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["NerService:Provider"] = "HuggingFace",
            ["NerService:TimeoutSeconds"] = "30",
            ["NerService:ModelUrl"] = "https://api-inference.huggingface.co/models/dslim/bert-base-NER",
            ["NerService:ApiKey"] = "test-api-key"
        });
        var services = new ServiceCollection();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var urlBuilder = serviceProvider.GetRequiredService<IWallapopUrlBuilder>();
        Assert.NotNull(urlBuilder);
    }

    [Fact]
    public void AddInfrastructure_NerServices_RegistersHttpClientForNerService()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["NerService:Provider"] = "HuggingFace",
            ["NerService:TimeoutSeconds"] = "30",
            ["NerService:ModelUrl"] = "https://api-inference.huggingface.co/models/dslim/bert-base-NER",
            ["NerService:ApiKey"] = "test-api-key"
        });

        var services = new ServiceCollection();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);
        var httpClient = httpClientFactory.CreateClient("NerService");
        Assert.NotNull(httpClient);
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }
}
