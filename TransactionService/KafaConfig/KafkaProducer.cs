
using System;
using System.Threading.Tasks;
using System.Linq;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

// NOTE: This class now ensures topics exist via the AdminClient before producing.

namespace TransactionService.KafaConfig;

public class KafkaProducer<TKey, TValue> : IDisposable
{
    static readonly int jitter = new Random().Next(0, 100);
    readonly IProducer<TKey, TValue> _producer;
    readonly ILogger<KafkaProducer<TKey, TValue>> _logger;


    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer<TKey, TValue>> logger)
    {
        _logger = logger;
        var bootstrap = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var kafkaProducerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrap,
            Acks = Acks.All,
            EnableIdempotence = true,
            RetryBackoffMs = 100 + jitter,
            EnableDeliveryReports = true
        };
        _producer = new ProducerBuilder<TKey, TValue>(kafkaProducerConfig)
            .SetErrorHandler((_, e) =>
                _logger.LogError("Kafka Producer Error: {Reason}", e.Reason))
        .Build();
    }

    public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic,
        TKey key,
        TValue value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<TKey, TValue>
            {
                Key = key,
                Value = value,
                Timestamp = Timestamp.Default
            };
            return await _producer.ProduceAsync(topic, message, cancellationToken);
        }
        catch (ProduceException<TKey, TValue> ex)
        {
            _logger.LogError("Failed to produce message to topic {Topic}: {Error}", topic, ex.Error.Reason);
            throw;
        }
    }

    public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, message, cancellationToken);
            
            _logger.LogInformation(
                "Message produced to {Topic} - Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition.Value,
                result.Offset.Value);

            return result;
        }
        catch (ProduceException<TKey, TValue> ex)
        {
            _logger.LogError(ex, 
                "Failed to produce message to topic {Topic}: {Reason}", 
                topic, 
                ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
