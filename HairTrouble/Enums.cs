namespace Destrospean.HairTrouble
{
    [System.Flags]
    public enum HairGrowthStateChangeFlags
    {
        Default,
        Haircut,
        NaturalGrowth
    }

    [System.Flags]
    public enum HairGrowthStates
    {
        Unknown,
        Any = 0x3F,
        Bald = 1,
        Shaved,
        Short = 4,
        ShortOrAbove = 0x3C,
        Medium = 8,
        MediumOrAbove = 0x38,
        Long = 0x10,
        LongOrAbove = 0x30,
        VeryLong = 0x20
    }
}
