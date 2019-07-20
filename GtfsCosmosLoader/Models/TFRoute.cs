using System.Collections.Generic;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Models
{
	public enum TFRouteType
	{
		Tram = 0,
		Subway = 1,
		Rail = 2,
		Bus = 3,
		Ferry = 4,
		CableCar = 5,
		Gondola = 6,
		Funicular = 7
	}

	public class TFRoute
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("recordType")]
		public string RecordType { get => "TFRoute"; }

		[JsonProperty("shortName")]
		public string ShortName { get; set; }

		[JsonProperty("longName")]
		public string LongName { get; set; }

		[JsonProperty("type")]
		public TFRouteType Type { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }
	}
}