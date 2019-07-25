using System;
using System.Globalization;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Converters
{
	public class DateOnlyConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTimeOffset);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = (string)reader.Value;
			return DateTimeOffset.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var date = (DateTimeOffset)value;
			writer.WriteValue(date.ToString("yyyy-MM-dd"));
		}
	}
}