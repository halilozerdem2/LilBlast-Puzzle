using System;

[Serializable]
public class PowerUpUsageSnapshot
{
    public int Shuffle;
    public int PowerShuffle;
    public int Manipulate;
    public int Destroy;

    public int TotalUsed => Shuffle + PowerShuffle + Manipulate + Destroy;

    public bool HasUsage => TotalUsed > 0;

    public PowerUpUsageSnapshot()
    {
    }

    public PowerUpUsageSnapshot(PowerUpUsageSnapshot other)
    {
        if (other == null)
            return;

        Shuffle = other.Shuffle;
        PowerShuffle = other.PowerShuffle;
        Manipulate = other.Manipulate;
        Destroy = other.Destroy;
    }
}
