using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using GtfsCosmosLoader.Converters;
using GtfsCosmosLoader.Models;

namespace GtfsCosmosLoader
{
	public class TransitFeedData
	{
		public List<TFTrip> Trips { get; set; }
		public HashSet<TFStop> Stops { get; set; }


		private Dictionary<string, TFRoute> _routes;
		private Dictionary<string, TFTrip> _trips;
		private Dictionary<string, TFStop> _stops;
		private Dictionary<string, TFServiceCalendar> _calendar;
		private Dictionary<string, List<TFServiceCalendarException>> _calendarExceptions;

		public TransitFeedData()
		{
			Trips = new List<TFTrip>();
			Stops = new HashSet<TFStop>();

			_routes = new Dictionary<string, TFRoute>();
			_trips = new Dictionary<string, TFTrip>();
			_stops = new Dictionary<string, TFStop>();
			_calendar = new Dictionary<string, TFServiceCalendar>();
			_calendarExceptions = new Dictionary<string, List<TFServiceCalendarException>>();
		}

		/// <summary>
		/// Reads GTFS (General/Google Transit Feed Specification) data from
		/// a given location, loading it in to the TransitFeed object for
		/// storage in a database.
		/// </summary>
		/// <remarks>
		/// The GTFS data that will be loaded looks like the following.
		///
		///  +--------+
		///  | Routes |
		///  +--------+
		///       ^                            +------+
		///       |                            |      |
		///  +--------+   +------------+   +-------+  |
		///  | Trips  |<--| Stop Times |-->| Stops |<-+
		///  +--------+   +------------+   +-------+
		///       |
		///       +-----------------+
		///       |                 |
		///       v                 v
		/// +----------+   +----------------+
		/// | Calendar |   | Calendar Dates |
		/// +----------+   +----------------+
		///
		/// To most easily add references to the data as it is loaded, it will
		/// be read in the following order (class names in brackets):
		///   - Routes (TFRoute)
		///   - Calendar (TFServiceCalendar)
		///   - Calendar Dates (TFServiceCalendarException)
		///   - Trips (TFTrip)
		///   - Stops (TFStop)
		///   - Stop Times (TFStopTime)
		/// </remarks>
		public void Read(string gtfsFilePath)
		{
			// Routes
			var routesPath = Path.Combine(gtfsFilePath, Constants.Files.Routes);
			var routesTime = ReadFile(routesPath, (csv) =>
			{
				var id = csv.GetField(Constants.Fields.RouteId);
				var route = new TFRoute
				{
					Id = id,
					ShortName = csv.GetField(Constants.Fields.RouteShortName).NullIfWhitespace(),
					LongName = csv.GetField(Constants.Fields.RouteLongName).NullIfWhitespace(),
					Type = csv.GetField<TFRouteType>(Constants.Fields.RouteType),
					Color = csv.GetField(Constants.Fields.RouteColor).NullIfWhitespace()
				};

				_routes.Add(id, route);
			});
			Console.WriteLine($"Done: Routes ({routesTime.TotalMilliseconds}ms)");

			// Calendar
			var calendarPath = Path.Combine(gtfsFilePath, Constants.Files.Calendar);
			var calendarTime = ReadFile(calendarPath, (csv) =>
			{
				var serviceId = csv.GetField(Constants.Fields.ServiceId);

				var startDate = DateTimeOffset.ParseExact(csv.GetField(Constants.Fields.StartDate), Constants.Formats.Date, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
				var endDate = DateTimeOffset.ParseExact(csv.GetField(Constants.Fields.EndDate), Constants.Formats.Date, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);

				var calendar = new TFServiceCalendar
				{
					Monday = csv.GetField<bool>(Constants.Fields.Monday),
					Tuesday = csv.GetField<bool>(Constants.Fields.Tuesday),
					Wednesday = csv.GetField<bool>(Constants.Fields.Wednesday),
					Thursday = csv.GetField<bool>(Constants.Fields.Thursday),
					Friday = csv.GetField<bool>(Constants.Fields.Friday),
					Saturday = csv.GetField<bool>(Constants.Fields.Saturday),
					Sunday = csv.GetField<bool>(Constants.Fields.Sunday),
					StartDate = startDate,
					EndDate = endDate
				};

				_calendar.Add(serviceId, calendar);
			});
			Console.WriteLine($"Done: Calendar ({calendarTime.TotalMilliseconds}ms)");

			// Calendar Date
			var calendarDatePath = Path.Combine(gtfsFilePath, Constants.Files.CalendarDates);
			var calendarDateTime = ReadFile(calendarDatePath, (csv) =>
			{
				var id = csv.GetField(Constants.Fields.ServiceId);

				if (!_calendarExceptions.ContainsKey(id))
				{
					_calendarExceptions[id] = new List<TFServiceCalendarException>();
				}

				var date = DateTimeOffset.ParseExact(csv.GetField(Constants.Fields.Date), Constants.Formats.Date, CultureInfo.InvariantCulture.DateTimeFormat);
				var exceptionType = csv.GetField<TFCalendarExceptionType>(Constants.Fields.ExceptionType);

				var exception = new TFServiceCalendarException
				{
					Date = date,
					ExceptionType = exceptionType
				};

				_calendarExceptions[id].Add(exception);
			});
			Console.WriteLine($"Done: Calendar Dates ({calendarDateTime.TotalMilliseconds}ms)");

			// Trips
			var tripsPath = Path.Combine(gtfsFilePath, Constants.Files.Trips);
			var tripsTime = ReadFile(tripsPath, (csv) =>
			{
				var id = csv.GetField(Constants.Fields.TripId);
				var serviceId = csv.GetField(Constants.Fields.ServiceId);

				var trip = new TFTrip
				{
					Id = id,
					Route = _routes[csv.GetField(Constants.Fields.RouteId)],
					Calendar = _calendar.GetValueOrDefault(serviceId),
					CalendarExceptions = _calendarExceptions.GetValueOrDefault(serviceId),
					Headsign = csv.GetField(Constants.Fields.TripHeadsign).NullIfWhitespace(),
					Direction = csv.GetField(Constants.Fields.DirectionId).NullIfWhitespace()
				};

				_trips.Add(id, trip);
				Trips.Add(trip);
			});
			Console.WriteLine($"Done: Trips ({tripsTime.TotalMilliseconds}ms)");

			// Stops
			var stopsPath = Path.Combine(gtfsFilePath, Constants.Files.Stops);
			var stopsTime = ReadFile(stopsPath, (csv) =>
			{
				var id = csv.GetField(Constants.Fields.StopId);
				var stop = new TFStop
				{
					Id = id,
					Code = csv.GetField(Constants.Fields.StopCode).NullIfWhitespace(),
					Name = csv.GetField(Constants.Fields.StopName).NullIfWhitespace(),
					Latitude = csv.GetField<float>(Constants.Fields.StopLat),
					Longitude = csv.GetField<float>(Constants.Fields.StopLon),
					Type = csv.GetField(Constants.Fields.LocationType).NullIfWhitespace(),
					ParentId = csv.GetField(Constants.Fields.ParentStation).NullIfWhitespace(),
					Platform = csv.GetField(Constants.Fields.PlatformCode).NullIfWhitespace()
				};

				_stops.Add(id, stop);
			});

			// Put a reference to the parent stop in each stop, and build out a
			// list of all of the top level stops.
			foreach (var stop in _stops)
			{
				var parentId = stop.Value.ParentId;
				stop.Value.ParentId = null;

				if (parentId is object)
				{
					// If this has a parent, add references between the parent 
					// and the child.
					var parent = _stops[parentId];
					stop.Value.Parent = parent;
					parent.Children.Add(stop.Value);
				}
				else
				{
					// If this does not have a parent, add it to the output
					// public list of stops
					Stops.Add(stop.Value);
				}
			}
			Console.WriteLine($"Done: Stops ({stopsTime.TotalMilliseconds}ms)");

			// Stop Times
			var stopTimesPath = Path.Combine(gtfsFilePath, Constants.Files.StopTimes);
			var stopTimesTime = ReadFile(stopTimesPath, (csv) =>
			{
				var stopId = csv.GetField(Constants.Fields.StopId);

				var stop = _stops[stopId];
				var trip = _trips[csv.GetField(Constants.Fields.TripId)];

				var stopTopLevel = HighestParentStop(stop);
				var stopTopLevelId = stopTopLevel.Id;

				var stopTime = new TFStopTime
				{
					StopTopLevel = stopTopLevelId,
					Stop = stopId,
					ArrivalTime = TimeSpanParser.ParseStopTime(csv.GetField(Constants.Fields.ArrivalTime)),
					DepartureTime = TimeSpanParser.ParseStopTime(csv.GetField(Constants.Fields.DepartureTime)),
					Sequence = csv.GetField<int>(Constants.Fields.StopSequence),
					PickupType = csv.GetField<TFPickupDropOffType>(Constants.Fields.PickupType),
					DropOffType = csv.GetField<TFPickupDropOffType>(Constants.Fields.DropOffType)
				};

				// StopTimes only relate to a stop and a time. Add a reference
				// for this StopTime to both of those. No list is maintained.
				trip.StopTimes.Add(stopTime);
			});

			Console.WriteLine($"Done: StopTimes ({stopTimesTime.TotalMilliseconds}ms)");
		}

		/// <summary>
		/// For a given stop, will follow the chain of parents up to the 
		/// highest level parent.
		/// </summary>
		public static TFStop HighestParentStop(TFStop stop)
		{
			if (stop.Parent is null)
			{
				return stop;
			}
			else
			{
				return HighestParentStop(stop.Parent);
			}
		}

		private static TimeSpan ReadFile(string fileName, Action<CsvReader> processLine)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			using (var reader = new StreamReader(fileName))
			using (var csv = new CsvReader(reader))
			{
				csv.Configuration.PrepareHeaderForMatch = (header, index) => header.ToLower();

				csv.Read();
				csv.ReadHeader();

				while (csv.Read())
				{
					processLine(csv);
				}
			}

			sw.Stop();
			return sw.Elapsed;
		}
	}
}