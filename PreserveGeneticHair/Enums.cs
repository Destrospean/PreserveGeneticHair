namespace Destrospean.PreserveGeneticHair
{
    public enum HairGrowthStates
    {
        Bald,
        Shaved,
        Short,
        Medium,
        Long,
        VeryLong
    }

    [System.Flags]
    public enum HairGrowthStateChangeFlags
    {
        Default,
        Haircut,
        NaturalGrowth
    }
}
