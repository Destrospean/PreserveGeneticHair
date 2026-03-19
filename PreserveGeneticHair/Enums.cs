namespace Destrospean.PreserveGeneticHair
{
    [System.Flags]
    public enum HairGrowthStateChangeFlags
    {
        Default,
        Haircut,
        NaturalGrowth
    }

    public enum HairGrowthStates
    {
        Bald,
        Shaved,
        Short,
        Medium,
        Long,
        VeryLong
    }
}
