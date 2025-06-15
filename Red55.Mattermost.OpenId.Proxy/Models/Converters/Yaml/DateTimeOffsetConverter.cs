using System.Globalization;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class DateTimeOffsetFormatConverter<T> : IYamlTypeConverter
{
    private string Format { get; init; }

    public DateTimeOffsetFormatConverter(string format)
    {
        Format = EnsureArg.IsNotNullOrWhiteSpace (format, nameof (format));
    }

    public bool Accepts(Type type) => type == typeof (T);

    public object ReadYaml(IParser parser, Type type)
    {
        var value = parser.Consume<Scalar> ().Value;
        return type == typeof (DateTimeOffset)
            ? DateTimeOffset.ParseExact (value, Format, CultureInfo.InvariantCulture)
            : DateOnly.ParseExact (value, Format, CultureInfo.InvariantCulture);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
        {
            emitter.Emit (new Scalar ("null"));
            return;
        }
        var o = (T)value!;

        var formattedValue = o switch
        {
            DateTimeOffset dto => dto.ToString (Format, CultureInfo.InvariantCulture),
            DateOnly dto => dto.ToString (Format, CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException ($"Unsupported type: {type.FullName}")
        };

        emitter.Emit (new Scalar (formattedValue));
    }
}

public class DateTimeOffsetConverter : DateTimeOffsetFormatConverter<DateTimeOffset>
{
    public DateTimeOffsetConverter() : base ("yyyy-MM-ddTHH:mm:ss.fffZ") { }
}

public class DateOnlyConverter : DateTimeOffsetFormatConverter<DateOnly>
{
    public DateOnlyConverter() : base ("yyyy-MM-dd") { }
}