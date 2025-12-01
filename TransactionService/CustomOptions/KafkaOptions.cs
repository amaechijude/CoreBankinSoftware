using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TransactionService.KafaConfig;

namespace TransactionService.CustomOptions;

public class KafkaOptions
{
    [Required, MinLength(1)]
    public List<string> BootstrapServers { get; set; } = [];
}

internal static class ServiceCollectionExtensions
{
    private static IServiceCollection AddKafkaOptions(this IServiceCollection services)
    {
        DotNetEnv.Env.TraversePath().Load();

        services.Configure<KafkaOptions>(options =>
        {
            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
                ?? throw new ServiceException("KAFKA_BOOTSTRAP_SERVERS environment variable is not set.");

            options.BootstrapServers = [.. bootstrapServers.Split(',').Select(s => s.Trim())];
        });

        services.AddOptions<KafkaOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddKafkaProducer(this IServiceCollection services)
    {
        services.AddSingleton(pr =>
        {
            var kafkaOptions = pr.GetRequiredService<IOptions<KafkaOptions>>().Value;

            var config = new ProducerConfig
            {
                BootstrapServers = string.Join(",", kafkaOptions.BootstrapServers),
                Acks = Acks.Leader,
                EnableIdempotence = true,
                RetryBackoffMs = 100,
                EnableDeliveryReports = true
            };

            var producer = new ProducerBuilder<string, string>(config).Build();

            return producer;
        });

        return services;
    }
    public static IServiceCollection AddCustomKafkaServiceExtentions(this IServiceCollection services)
    {
        services.AddKafkaOptions();
        services.AddKafkaProducer();

        return services;
    }
}


internal class ServiceException(string message) : Exception(message);