using System;
using System.Collections.Generic;
using Destrospean.PreserveGeneticHair;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.HairTrouble
{
    public static class SimHairGrowth
    {
        static Dictionary<string, HairGrowthStates> sHairGrowthStateMap;

        public static Dictionary<string, HairGrowthStates> HairGrowthStateMap
        {
            get
            {
                if (sHairGrowthStateMap == null)
                {
                    InitHairGrowthStateMap();
                }
                return sHairGrowthStateMap;
            }
        }

        public static void ApplyHairstylesWithGrowthStateToAllOutfits(this SimDescription simDescription, HairGrowthStates hairGrowthState, bool spin = false)
        {
            simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) => ApplyHairstyleWithGrowthStateToOutfit(simDescription, simBuilder, outfitCategory, outfitIndex, hairGrowthState), spin);
        }

        public static SimOutfit ApplyHairstyleWithGrowthStateToOutfit(this SimDescription simDescription, SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex, HairGrowthStates hairGrowthState)
        {
            HairGrowthStates outfitHairGrowthState;
            if (simDescription.GetOutfit(outfitCategory, outfitIndex).TryGetHairGrowthState(out outfitHairGrowthState) && (outfitHairGrowthState & hairGrowthState) != 0)
            {
                return null;
            }
            List<CASPart> validHairCASPs = new List<CASPart>();
            foreach (KeyValuePair<string, HairGrowthStates> hairGrowthStateMapKvp in HairGrowthStateMap)
            {
                if (hairGrowthStateMapKvp.Value == hairGrowthState)
                {
                    CASPart casPart = new CASPart(Common.FromS3PIFormatKeyString(hairGrowthStateMapKvp.Key));
                    if (casPart.Key != ResourceKey.kInvalidResourceKey && (casPart.Age & simDescription.Age) != 0 && (casPart.Gender & simDescription.Gender) != 0 && (casPart.Species & simDescription.Species) != 0 && (casPart.CategoryFlags & (uint)outfitCategory) != 0 /*&& (casPart.CategoryFlags & (uint)OutfitCategoriesExtended.ValidForRandom) != 0*/ && (casPart.CategoryFlags & (uint)(OutfitCategoriesExtended.IsHat | OutfitCategoriesExtended.IsHiddenInCAS)) == 0)
                    {
                        validHairCASPs.Add(casPart);
                    }
                }
            }
            if (validHairCASPs.Count == 0)
            {
                return null;
            }
            simBuilder.PrepareForOutfit(simDescription.GetOutfit(outfitCategory, outfitIndex));
            simBuilder.RemoveParts(BodyTypes.Hair);
            simBuilder.AddPart(validHairCASPs[Sims3.Gameplay.Core.RandomUtil.GetInt(0, validHairCASPs.Count - 1)]);
            OutfitUtils.InjectBodyHairColor(simBuilder, simDescription.BodyHairColor.ActiveColor);
            OutfitUtils.InjectEyeBrowHairColor(simBuilder, simDescription.EyebrowColor.ActiveColor);
            OutfitUtils.InjectHairColor(simBuilder, Array.ConvertAll(simDescription.FacialHairColors, x => x.ActiveColor), BodyTypes.Beard);
            OutfitUtils.InjectHairColor(simBuilder, Array.ConvertAll(simDescription.HairColors, x => x.ActiveColor), BodyTypes.Hair);
            return new SimOutfit(simBuilder.CacheOutfit(string.Format("Rebuilt_{0}_{1}", outfitCategory, outfitIndex)));
        }

        public static bool DecrementHairGrowthState(this SimDescription simDescription, int by = 1, bool haircut = true, HairGrowthStateChangeFlags additionalFlags = 0)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() >> by;
            List<int> growthStates = new List<int>();
            foreach (int value in Enum.GetValues(typeof(HairGrowthStates)))
            {
                growthStates.Add(value);
            }
            growthStates.Sort();
            if (newHairGrowthState < growthStates.FindLast(x => x > 0 && (x & (x - 1)) == 0))
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
            return 0;
        }

        public static HairGrowthStates GetHairGrowthState(this SimDescription simDescription)
        {
            HairGrowthStates hairGrowthState;
            if (simDescription.TryGetHairGrowthState(out hairGrowthState))
            {
                return hairGrowthState;
            }
            if (simDescription.GetOutfit(simDescription.CreatedSim == null ? OutfitCategories.Everyday : simDescription.CreatedSim.CurrentOutfitCategory, simDescription.CreatedSim == null ? 0 : simDescription.CreatedSim.CurrentOutfitIndex).TryGetHairGrowthState(out hairGrowthState))
            {
                simDescription.SetHairGrowthState(hairGrowthState);
                return simDescription.GetHairGrowthState();
            }
            return 0;
        }

        public static bool IncrementHairGrowthState(this SimDescription simDescription, int by = 1, bool naturalGrowth = true, HairGrowthStateChangeFlags additionalFlags = 0)
        {
            if (by < 1)
            {
                return false;
            }
            int newHairGrowthState = (int)simDescription.GetHairGrowthState() << by;
            if (newHairGrowthState > 1)
            {
                simDescription.SetHairGrowthState(newHairGrowthState, (naturalGrowth ? HairGrowthStateChangeFlags.NaturalGrowth : HairGrowthStateChangeFlags.Default) | additionalFlags);
                return true;
            }
            return false;
        }

        public static void InitHairGrowthStateMap()
        {
            sHairGrowthStateMap = new Dictionary<string, HairGrowthStates>();
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType("Destrospean.HairTrouble.Data") == null)
                {
                    continue;
                }
                System.Xml.XmlReader reader = Simulator.ReadXml(new ResourceKey(ResourceUtils.HashString64(assembly.GetName().Name), 0x333406C, 0));
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        if (reader.Name == "HairGrowthStateMap")
                        {
                            reader.MoveToContent();
                        }
                        else if (reader.Name == "Entry")
                        {
                            sHairGrowthStateMap[reader.GetAttribute("CASPartKey")] = (HairGrowthStates)Enum.Parse(typeof(HairGrowthStates), reader.GetAttribute("GrowthState"));
                        }
                    }
                }
                reader.Close();
            }
        }

        public static void OnHairGrowthStateChanged(this SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags)
        {
            simDescription.ApplyHairstylesWithGrowthStateToAllOutfits(hairGrowthState);
            if (!simDescription.HasRootsShowing() && (flags & HairGrowthStateChangeFlags.NaturalGrowth) != 0 && !simDescription.HasOriginalHairColors())
            {
                simDescription.HasRootsShowing(true);
            }
        }

        public static void SetHairGrowthState(this SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags = 0)
        {
            HairGrowthStates longestHairGrowthState = 0;
            foreach (int value in Enum.GetValues(typeof(HairGrowthStates)))
            {
                if (value > 0 && (value & (value - 1)) == 0 && ((int)hairGrowthState & value) != 0)
                {
                    longestHairGrowthState = (HairGrowthStates)value;
                }
            }
            SimHairData.GrowthStates[simDescription.SimDescriptionId] = longestHairGrowthState;
            simDescription.OnHairGrowthStateChanged(longestHairGrowthState, flags);
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
            int partIndex = Array.FindIndex(outfit.Parts, x => x.BodyType == BodyTypes.Hair);
            if (partIndex == -1)
            {
                hairGrowthState = 0;
                part = null;
                return false;
            }
            return (part = outfit.Parts[partIndex]).Value.TryGetHairGrowthState(out hairGrowthState);
        }

        public static bool TryGetHairGrowthState(this CASPart part, out HairGrowthStates hairGrowthState)
        {
            return HairGrowthStateMap.TryGetValue(part.Key.ToS3PIFormatKeyString(), out hairGrowthState);
        }

        public static bool TryGetHairGrowthState(this SimDescription simDescription, out HairGrowthStates hairGrowthState)
        {
            return SimHairData.GrowthStates.TryGetValue(simDescription.SimDescriptionId, out hairGrowthState);
        }
    }
}
