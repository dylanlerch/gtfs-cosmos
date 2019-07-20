using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GtfsCosmosLoader.Models
{
	public enum TFCalendarExceptionType
	{
		Added = 1,
		Removed = 2
	}

	public class TFServiceCalendarException
	{
		[JsonProperty("date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("exceptionType")]
		public TFCalendarExceptionType ExceptionType { get; set; }
	}
}