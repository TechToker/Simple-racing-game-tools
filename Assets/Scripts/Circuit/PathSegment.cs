using UnityEngine;

public class PathSegment
{
    public WayPoint Waypoint { get; }
    public Vector3 CarRacingPoint { get; private set; }

    public PathSegment(DriverAI driver, WayPoint wp)
    {
        Waypoint = wp;
            
        float wpWidthWithExtraOffset = wp.Width - driver.Car.CarSize.x;
        float driverTargetPoint = !driver.OvertakeMode ? wp.LocalFinalRacingPoint : wp.LocalOvertakeRacingPoint;

        //Lock local racing point in new waypoint width
        float newLocalRp = Mathf.Clamp(driverTargetPoint, -wpWidthWithExtraOffset / wp.Width / 2, wpWidthWithExtraOffset / wp.Width / 2);

        CarRacingPoint = wp.transform.TransformPoint(newLocalRp * wp.Width, 0.5f, 0);
    }
}
