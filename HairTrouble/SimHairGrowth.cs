using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.HairTrouble
{
    public static class SimHairGrowth
    {
        static Dictionary<string, HairGrowthStates> sHairGrowthStateMap;

        public static event EventHandler<HairGrowthStateChangedEventArgs> HairGrowthStateChanged;

        public static Dictionary<string, HairGrowthStates> HairGrowthStateMap
        {
            get
            {
                if (sHairGrowthStateMap == null)
                {
                    sHairGrowthStateMap = new Dictionary<string, HairGrowthStates>();
                    foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.GetType("Destrospean.HairTrouble.Data") == null)
                        {
                            continue;
                        }
                        foreach (System.Xml.XmlNode node in Simulator.LoadFromResourceKey(new ResourceKey(ResourceUtils.HashString64(assembly.GetName().Name), 0x333406C, 0)).SelectSingleNode("HairGrowthStateMap").ChildNodes)
                        {
                            if (node.Name == "Entry")
                            {
                                sHairGrowthStateMap[node.Attributes["CASPartKey"].Value] = (HairGrowthStates)Enum.Parse(typeof(HairGrowthStates), node.Attributes["GrowthState"].Value);
                            }
                        }
                    }
                }
                return sHairGrowthStateMap;
            }
        }

        public class HairGrowthStateChangedEventArgs : EventArgs
        {
            public HairGrowthStateChangeFlags Flags
            {
                get;
            }

            public HairGrowthStates HairGrowthState
            {
                get;
            }

            public SimDescription SimDescription
            {
                get;
            }

            public HairGrowthStateChangedEventArgs(SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags)
            {
                Flags = flags;
                HairGrowthState = hairGrowthState;
                SimDescription = simDescription;
            }

            public HairGrowthStateChangedEventArgs(SimDescription simDescription, int hairGrowthState, HairGrowthStateChangeFlags flags) : this(simDescription, (HairGrowthStates)hairGrowthState, flags)
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
            if (newHairGrowthState < Enum.GetValues(typeof(HairGrowthStates)).Length - 1)
            {
                simDescription.SetHairGrowthState(newHairGrowthState, (haircut ? HairGrowthStateChangeFlags.Haircut : HairGrowthStateChangeFlags.Default) | additionalFlags);
                return true;
            }
            return false;
        }

        public static HairGrowthStates GetHairGrowthState(this SimOutfit outfit)
        {
            HairGrowthStates hairGrowthState;
            CASPart? part;
            if (outfit.TryGetHairGrowthState(out hairGrowthState, out part))
            {
                return hairGrowthState;
            }
            if (part == null)
            {
                throw new NullReferenceException(string.Format("No CASPart could be found in the specified SimOutfit (Key: {0}).", outfit.Key.ToS3PIFormatKeyString()));
            }
            throw new KeyNotFoundException(string.Format("No HairGrowthStates could be found for the specified CASPart (Key: {0}).", part.Value.Key.ToS3PIFormatKeyString()));
        }

        public static HairGrowthStates GetHairGrowthState(this SimDescription simDescription, OutfitCategories? outfitCategory = null, int? outfitIndex = null)
        {
            HairGrowthStates hairGrowthState;
            if (simDescription.TryGetHairGrowthState(out hairGrowthState))
            {
                return hairGrowthState;
            }
            SimOutfit outfit;
            CASPart? part;
            if ((outfit = simDescription.GetOutfit(outfitCategory ?? (simDescription.CreatedSim == null ? OutfitCategories.Everyday : simDescription.CreatedSim.CurrentOutfitCategory), outfitIndex ?? (simDescription.CreatedSim == null ? 0 : simDescription.CreatedSim.CurrentOutfitIndex))).TryGetHairGrowthState(out hairGrowthState, out part))
            {
                simDescription.SetHairGrowthState(hairGrowthState);
                return hairGrowthState;
            }
            if (part == null)
            {
                throw new NullReferenceException(string.Format("No CASPart could be found in the specified SimOutfit (Key: {0}).", outfit.Key.ToS3PIFormatKeyString()));
            }
            throw new KeyNotFoundException(string.Format("No HairGrowthStates could be found for the Sim (Name: {0}, SimDescriptionId: {1}) or for the specified CASPart (Key: {2}).", simDescription.FullName, simDescription.SimDescriptionId, part.Value.Key.ToS3PIFormatKeyString()));
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

        public static void OnHairGrowthStateChanged(this SimDescription simDescription, HairGrowthStateChangedEventArgs e)
        {
            HairGrowthStateChanged.Invoke(simDescription, e);
        }

        public static void SetHairGrowthState(this SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags = 0)
        {
            SimHairData.GrowthStates[simDescription.SimDescriptionId] = hairGrowthState;
            simDescription.OnHairGrowthStateChanged(new HairGrowthStateChangedEventArgs(simDescription, hairGrowthState, flags));
        }

        public static void SetHairGrowthState(this SimDescription simDescription, int hairGrowthState, HairGrowthStateChangeFlags flags = 0)
        {
            simDescription.SetHairGrowthState((HairGrowthStates)hairGrowthState, flags);
        }

        public static bool TryGetHairGrowthState(this SimOutfit outfit, out HairGrowthStates hairGrowthState)
        {
            CASPart? part;
            return outfit.TryGetHairGrowthState(out hairGrowthState, out part);
        }

        public static bool TryGetHairGrowthState(this SimOutfit outfit, out HairGrowthStates hairGrowthState, out CASPart? part)
        {
            CASPart? tempPart = null;
            if (SimHairGrowth.HairGrowthStateMap.TryGetValue(Array.Find(outfit.Parts, x =>
                {
                    if (x.BodyType == BodyTypes.Hair)
                    {
                        tempPart = x;
                        return true;
                    }
                    return false;
                }).Key.ToS3PIFormatKeyString(), out hairGrowthState))
            {
                part = tempPart;
                return true;
            }
            part = tempPart;
            return false;
        }

        public static bool TryGetHairGrowthState(this SimDescription simDescription, out HairGrowthStates hairGrowthState)
        {
            return SimHairData.GrowthStates.TryGetValue(simDescription.SimDescriptionId, out hairGrowthState);
        }
    }
}
