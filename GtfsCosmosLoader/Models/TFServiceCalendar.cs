using System;
using GtfsCosmosLoader.Converters;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Models
{
	public class TFServiceCalendar
	{
		[JsonProperty("monday")]
		public bool Monday { get; set; }

		[JsonProperty("tuesday")]
		public bool Tuesday { get; set; }

		[JsonProperty("wednesday")]
		public bool Wednesday { get; set; }

		[JsonProperty("thursday")]
		public bool Thursday { get; set; }

		[JsonProperty("friday")]
		public bool Friday { get; set; }

		[JsonProperty("saturday")]
		public bool Saturday { get; set; }

		[JsonProperty("sunday")]
		public bool Sunday { get; set; }

		[JsonProperty("startDate")]
		[JsonConverter(typeof(DateOnlyConverter))]
		public DateTimeOffset StartDate { get; set; }

		[JsonProperty("endDate")]
		[JsonConverter(typeof(DateOnlyConverter))]
		public DateTimeOffset EndDate { get; set; }
	}
}