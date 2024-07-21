public class HydrolicErosionSettings
{
    public float ErosionRate { get; private set; }
    public float DepositRate { get; private set; }
    public float MaxErosionAmt { get; private set; }
    public int Iterations { get; private set; }
    public int WindSimulationInterval { get; private set; }
    public int Size { get; set; }
    public int DropletLifetime { get; set; }

    public HydrolicErosionSettings(int iterations = 50000, int windSimulationCount = 10, int dropletLifetime = 20,
            float erosionRate = 0.01f, float depositRate = 0.001f, float maxErosionAmt = 0.01f)
    {
        this.DropletLifetime = dropletLifetime;
        this.ErosionRate = erosionRate;
        this.DepositRate = depositRate;
        this.MaxErosionAmt = maxErosionAmt;
        this.Iterations = iterations;
        if (iterations == 0)
        {
            this.WindSimulationInterval = int.MaxValue;
        }
        else
        {
            this.WindSimulationInterval = iterations / windSimulationCount;
        }

    }
}