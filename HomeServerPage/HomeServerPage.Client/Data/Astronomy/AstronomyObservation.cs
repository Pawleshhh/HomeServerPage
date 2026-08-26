using AstroCalc.Observation;

namespace HomeServerPage.Data.Astronomy;

public sealed record AstronomyObservation(
    string Name,
    string Key,
    RiseTransitSetResult TransitResult);
