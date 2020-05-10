
public class BrakingData
{
    public bool IsRecording;
    public float CornerEnterSpeed { get; }
    public float CornerExitSpeed { get; }
    public float StartBrakingDistance { get; }
        
    //Lifetime graph-X
    public float CurrentDistanceProgress { get; private set; }
    //Lifetime graph-Y
    public float CurrentBrakingSpeed { get; private set; }
        
    public BrakingData(float enterSpeed, float exitSpeed, float brakingDistance)
    {
        IsRecording = true;
        CornerEnterSpeed = enterSpeed;
        CornerExitSpeed = exitSpeed;
        StartBrakingDistance = brakingDistance;
    }

    public void SetCurrentData(float currentDistance, float currentSpeed)
    {
        CurrentDistanceProgress = currentDistance;
        CurrentBrakingSpeed = currentSpeed;
    }
}