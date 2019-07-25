using System;
using System.Globalization;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Converters
{
	public class TimeSpanConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TimeSpan);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = (string)reader.Value;
			return TimeSpanParser.ParseStopTime(value);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var time = (TimeSpan)value;

			writer.WriteValue($"{(int)time.TotalHours}:{time.ToString("mm")}:{time.ToString("ss")}");
		}
	}
}