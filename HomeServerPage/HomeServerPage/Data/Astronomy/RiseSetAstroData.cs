using AstroCalc.Coordinates;
using AstroCalc.Core;
using AstroCalc.SolarSystem;
using System.Collections;

namespace HomeServerPage.Data.Astronomy;

public record RiseSetAstroData(
    SolarSystemObject SolarSystemObject,
    DateTime RiseDateTime,
    DateTime SetDateTime,
    HorizontalCoordinate HorizontalCoordinate,
    EquatorialCoordinate EquatorialCoordinate,
    GeographicCoordinate ObserverLocation);

public enum SolarSystemObject
{
    Sun,
    Mercury,
    Venus,
    Earth,
    EarthMoon,
    Mars,
    Jupiter,
    Saturn,
    Uranus,
    Neptune
}

public static class SolarSystemObjectExtensions
{
    extension(SolarSystemObject solarObject)
    {

        public Planet? ToPlanet()
        {
            return solarObject switch
            {
                SolarSystemObject.Mercury => Planet.Mercury,
                SolarSystemObject.Venus => Planet.Venus,
                SolarSystemObject.Earth => Planet.Earth,
                SolarSystemObject.Mars => Planet.Mars,
                SolarSystemObject.Jupiter => Planet.Jupiter,
                SolarSystemObject.Saturn => Planet.Saturn,
                SolarSystemObject.Uranus => Planet.Uranus,
                SolarSystemObject.Neptune => Planet.Neptune,
                _ => null
            };
        }

        public bool IsSun() => solarObject is SolarSystemObject.Sun;
        public bool IsMoon() => solarObject is SolarSystemObject.EarthMoon;

    }
}