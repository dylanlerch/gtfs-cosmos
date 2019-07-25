using System;

namespace GtfsCosmosLoader.Converters
{
	public static class TimeSpanParser
	{
		/// <summary>
		/// GTFS can have stop times that are greater than 24 hours (for trips
		/// that finish the day after they start). Manually parsing as 
		/// TimeSpan.Parse can't that.
		///
		/// Format will be either H:MM:SS or HH:MM:SS.
		/// </summary>
		public static TimeSpan? ParseStopTime(string time)
		{
			if (string.IsNullOrWhiteSpace(time))
			{
				return null;
			}
			else 
			{
				var splitTime = time.Split(':');

				var hours = int.Parse(splitTime[0]);
				var minutes = int.Parse(splitTime[1]);
				var seconds = int.Parse(splitTime[2]);

				return new TimeSpan(hours, minutes, seconds);
			}
		}
	}
}