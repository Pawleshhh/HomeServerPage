using AstroCalc.Catalogs;
using AstroCalc.Coordinates;
using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.Time;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeServerPage.Shared.Extensions;

public static class AstroExtensions
{
    public static string FormatUtHours(this double utHours, DateTime requestDateUtc, TimeZoneInfo localTimeZone)
    {
        if (double.IsNaN(utHours))
        {
            return "—";
        }

        var utcDateTime = DateTime.SpecifyKind(requestDateUtc.Date.AddHours(utHours), DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone).ToString("HH:mm");
    }

    extension(MessierObject messierObject)
    {
        public HorizontalCoordinate HorizontalCoordinate(DateTime dateTime, GeographicCoordinate location)
        {
            var jd = JulianDate.FromDateTime(dateTime);
            var lst = AstroCalc.Time.SiderealTime.LocalMeanSiderealTime(jd, location.Longitude);
            return CoordinateTransform.EquatorialToHorizontal(messierObject.Position, lst, location.Latitude);
        }
    }
}
