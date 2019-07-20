using System.Collections.Generic;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Models
{
	public class TFTrip
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("recordType")]
		public string RecordType { get => "TFTrip"; }

		[JsonProperty("route")]
		public TFRoute Route { get; set; }

		[JsonProperty("stopTimes")]
		public List<TFStopTime> StopTimes { get; set; } = new List<TFStopTime>();

		[JsonProperty("calendar")]
		public TFServiceCalendar Calendar { get; set; }

		[JsonProperty("calendarExceptions")]
		public List<TFServiceCalendarException> CalendarExceptions { get; set; }

		[JsonProperty("headsign")]
		public string Headsign { get; set; }

		[JsonProperty("direction")]
		public string Direction { get; set; }
	}
}