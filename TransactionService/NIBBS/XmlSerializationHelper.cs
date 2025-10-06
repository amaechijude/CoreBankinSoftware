using System.Collections.Concurrent;
using System.Xml.Serialization;

namespace TransactionService.NIBBS;


internal static class XmlSerializationHelper
{
    private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializerCache = new();

    private static XmlSerializer GetSerializer(Type type)
    {
        return SerializerCache.GetOrAdd(type, t => new XmlSerializer(t));
    }

    public static string Serialize<T>(T obj) where T : class
    {
        var serializer = GetSerializer(typeof(T));
        using var writer = new StringWriter();
        serializer.Serialize(writer, obj);
        return writer.ToString();
    }

    public static T? Deserialize<T>(string xml)
    {
        var serializer = GetSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T?)serializer.Deserialize(reader);
    }
}