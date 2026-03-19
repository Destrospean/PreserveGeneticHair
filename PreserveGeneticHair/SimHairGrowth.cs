using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public static class SimHairGrowth
    {
        public static Dictionary<string, HairGrowthStates> StateCASPartKeyMap = new Dictionary<string, HairGrowthStates>
        {
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("afHairEP08Hairless"),
                HairGrowthStates.Bald
            },
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("amHairEP08Hairless"),
                HairGrowthStates.Bald
            },
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("cfHairEP08Hairless"),
                HairGrowthStates.Bald
            },
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("cmHairEP08Hairless"),
                HairGrowthStates.Bald
            },
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("efHairEP08Hairless"),
                HairGrowthStates.Bald
            },
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("emHairEP08Hairless"),
                HairGrowthStates.Bald
            },
            {
                "034AEECB:00000000:" + ResourceUtils.HashString64("puHairNone"),
                HairGrowthStates.Bald
            }
        };

        public static event System.EventHandler<StateChangedEventArgs> StateChanged;

        public class StateChangedEventArgs : System.EventArgs
        {
            public HairGrowthStateChangeFlags Flags
            {
                get;
            }

            public HairGrowthStates State
            {
                get;
            }

            public SimDescription SimDescription
            {
                get;
            }

            public StateChangedEventArgs(SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags)
            {
                Flags = flags;
                State = hairGrowthState;
                SimDescription = simDescription;
            }

            public StateChangedEventArgs(SimDescription simDescription, int hairGrowthState, HairGrowthStateChangeFlags flags) : this(simDescription, (HairGrowthStates)hairGrowthState, flags)
            {
            }
        }

        public static bool DecrementHairGrowthState(this SimDescription simDescription, int by = 1, bool haircut = true, HairGrowthStateChangeFlags additionalFlags = 0)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() + by;
            if (newHairGrowthState < System.Enum.GetValues(typeof(HairGrowthStates)).Length - 1)
            {
                simDescription.SetHairGrowthState(newHairGrowthState, (haircut ? HairGrowthStateChangeFlags.Haircut : HairGrowthStateChangeFlags.Default) | additionalFlags);
                return true;
            }
            return false;
        }

        public static HairGrowthStates GetHairGrowthState(this SimDescription simDescription, OutfitCategories? outfitCategory = null, int? outfitIndex = null)
        {
            HairGrowthStates hairGrowthState;
            if (SimHairData.GrowthStates.TryGetValue(simDescription.SimDescriptionId, out hairGrowthState))
            {
                return hairGrowthState;
            }
            if (StateCASPartKeyMap.TryGetValue(System.Array.Find(simDescription.GetOutfit(outfitCategory == null ? simDescription.CreatedSim == null ? OutfitCategories.Everyday : simDescription.CreatedSim.CurrentOutfitCategory : outfitCategory.Value, outfitIndex == null ? simDescription.CreatedSim == null ? 0 : simDescription.CreatedSim.CurrentOutfitIndex : outfitIndex.Value).Parts, x => x.BodyType == BodyTypes.Hair).Key.ToString(), out hairGrowthState))
            {
                simDescription.SetHairGrowthState(hairGrowthState);
                return hairGrowthState;
            }
            simDescription.SetHairGrowthState(0);
            return 0;
        }

        public static bool IncrementHairGrowthState(this SimDescription simDescription, int by = 1, bool naturalGrowth = true, HairGrowthStateChangeFlags additionalFlags = 0)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() - by;
            if (newHairGrowthState > 0)
            {
                simDescription.SetHairGrowthState(newHairGrowthState, (naturalGrowth ? HairGrowthStateChangeFlags.NaturalGrowth : HairGrowthStateChangeFlags.Default) | additionalFlags);
                return true;
            }
            return false;
        }

        public static void OnHairGrowthStateChanged(this SimDescription simDescription, StateChangedEventArgs e)
        {
            StateChanged.Invoke(simDescription, e);
        }

        public static void SetHairGrowthState(this SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags = 0)
        {
            SimHairData.GrowthStates[simDescription.SimDescriptionId] = hairGrowthState;
            simDescription.OnHairGrowthStateChanged(new StateChangedEventArgs(simDescription, hairGrowthState, flags));
        }

        public static void SetHairGrowthState(this SimDescription simDescription, int hairGrowthState, HairGrowthStateChangeFlags flags = 0)
        {
            simDescription.SetHairGrowthState((HairGrowthStates)hairGrowthState, flags);
        }
    }
}
