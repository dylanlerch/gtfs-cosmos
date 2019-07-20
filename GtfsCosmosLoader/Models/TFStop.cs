using System.Collections.Generic;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Models
{
	public class TFStop
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("recordType")]
		public string RecordType { get => "TFStop"; }

		[JsonProperty("code")]
		public string Code { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("latitude")]
		public float Latitude { get; set; }

		[JsonProperty("longitude")]
		public float Longitude { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("parentId")]
		public string ParentId { get; set; }

		[JsonProperty("platform")]
		public string Platform { get; set; }

		[JsonProperty("children")]
		public HashSet<TFStop> Children { get; set; } = new HashSet<TFStop>();

		[JsonIgnore]
		public TFStop Parent { get; set; }
	}
}