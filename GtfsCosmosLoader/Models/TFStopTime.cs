using Newtonsoft.Json;

namespace GtfsCosmosLoader.Models
{
	public enum TFPickupDropOffType
	{
		RegularlyScheduled = 0,
		NotAvailable = 1,
		PhoneAgency = 2,
		CoordinateWithDriver = 3
	}
	
	public class TFStopTime
	{
		[JsonProperty("stopTopLevel")]
		public string StopTopLevel { get; set; }

		[JsonProperty("stop")]
		public string Stop { get; set; }

		[JsonProperty("arrivalTime")]
		public string ArrivalTime { get; set; }

		[JsonProperty("departureTime")]
		public string DepartureTime { get; set; }

		[JsonProperty("sequence")]
		public int Sequence { get; set; }

		[JsonProperty("pickupType")]
		public TFPickupDropOffType PickupType { get; set; }

		[JsonProperty("dropOffType")]
		public TFPickupDropOffType DropOffType { get; set; }
	}
}