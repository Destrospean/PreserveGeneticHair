using System.Collections.Generic;
using Sims3.Gameplay.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public static class SimHairGrowth
    {
        public static bool DecrementHairGrowthState(this SimDescription simDescription, int by = 1)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() + by;
            if (newHairGrowthState < System.Enum.GetValues(typeof(HairGrowthStates)).Length)
            {
                simDescription.SetHairGrowthState(newHairGrowthState);
                return true;
            }
            return false;
        }

        public static HairGrowthStates GetHairGrowthState(this SimDescription simDescription)
        {
            HairGrowthStates hairGrowthState;
            return SimHairData.HairGrowthStates.TryGetValue(simDescription.SimDescriptionId, out hairGrowthState) ? hairGrowthState : SimHairData.HairGrowthStates[simDescription.SimDescriptionId] = 0;
        }

        public static bool IncrementHairGrowthState(this SimDescription simDescription, int by = 1)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() - by;
            if (newHairGrowthState > 0)
            {
                simDescription.SetHairGrowthState(newHairGrowthState);
                return true;
            }
            return false;
        }

        public static void SetHairGrowthState(this SimDescription simDescription, HairGrowthStates hairGrowthState)
        {
            SimHairData.HairGrowthStates[simDescription.SimDescriptionId] = hairGrowthState;
        }

        public static void SetHairGrowthState(this SimDescription simDescription, int hairGrowthState)
        {
            simDescription.SetHairGrowthState((HairGrowthStates)hairGrowthState);
        }
    }
}
