using Sims3.Gameplay.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public static class SimHairGrowth
    {
        public static HairGrowthStates GetHairGrowthState(this SimDescription simDescription)
        {
            HairGrowthStates hairGrowthState;
            return SimHairData.HairGrowthStates.TryGetValue(simDescription.SimDescriptionId, out hairGrowthState) ? hairGrowthState : SimHairData.HairGrowthStates[simDescription.SimDescriptionId] = 0;
        }

        public static bool LowerHairGrowthState(this SimDescription simDescription, int by = 1)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() - by;
            if (newHairGrowthState > 0)
            {
                simDescription.SetHairGrowthState((HairGrowthStates)newHairGrowthState);
                return true;
            }
            return false;
        }

        public static bool RaiseHairGrowthState(this SimDescription simDescription, int by = 1)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() + by;
            if (newHairGrowthState < System.Enum.GetValues(typeof(HairGrowthStates)).Length)
            {
                simDescription.SetHairGrowthState((HairGrowthStates)newHairGrowthState);
                return true;
            }
            return false;
        }

        public static void SetHairGrowthState(this SimDescription simDescription, HairGrowthStates hairGrowthState)
        {
            SimHairData.HairGrowthStates[simDescription.SimDescriptionId] = hairGrowthState;
        }
    }
}
