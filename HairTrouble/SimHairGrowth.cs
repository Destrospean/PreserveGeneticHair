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
                return sHairGrowthStateMap;
            }
        }

        public static void ApplyHairstylesWithGrowthStateToAllOutfits(this SimDescription simDescription, HairGrowthStates hairGrowthState, bool spin = false)
        {
            SimBuilder simBuilder = new SimBuilder
                {
                    UseCompression = true
                };
            OutfitCategories lastOutfitCategory = 0;
            int lastOutfitIndex = 0,
            tempOutfitIndex = 0;
            if (simDescription.CreatedSim != null)
            {
                lastOutfitCategory = simDescription.CreatedSim.CurrentOutfitCategory;
                lastOutfitIndex = simDescription.CreatedSim.CurrentOutfitIndex;
                tempOutfitIndex = simDescription.GetOutfitCount(OutfitCategories.Everyday);
                simDescription.AddOutfit(new SimOutfit(simDescription.CreatedSim.CurrentOutfit.Key), OutfitCategories.Everyday);
                simDescription.CreatedSim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, tempOutfitIndex);
                ApplyHairstyleWithGrowthStateToOutfit(simDescription, simBuilder, lastOutfitCategory, lastOutfitIndex, hairGrowthState);
                if (spin)
                {
                    using (Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper switchOutfitHelper = new Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper(simDescription.CreatedSim, Sims3.Gameplay.Actors.Sim.ClothesChangeReason.Force, lastOutfitCategory, lastOutfitIndex, false))
                    {
                        simDescription.CreatedSim.SwitchToOutfitWithSpin(switchOutfitHelper);
                    }
                }
                else
                {
                    simDescription.CreatedSim.SwitchToOutfitWithoutSpin(lastOutfitCategory, lastOutfitIndex);
                }
                simDescription.RemoveOutfit(OutfitCategories.Everyday, tempOutfitIndex, true);
            }
            Dictionary<uint, int> specialOutfitIndices = new Dictionary<uint, int>();
            foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in simDescription.mSpecialOutfitIndices)
            {
                specialOutfitIndices.Add(specialOutfitIndexKvp.Key, simDescription.mSpecialOutfitIndices.Count - 1 - specialOutfitIndexKvp.Value);
            }
            foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
            {
                for (int i = simDescription.GetOutfitCount(outfitCategory) - 1; i > -1 ; i--)
                {
                    if (simDescription.CreatedSim == null || outfitCategory != lastOutfitCategory || i != lastOutfitIndex)
                    {
                        ApplyHairstyleWithGrowthStateToOutfit(simDescription, simBuilder, outfitCategory, i, hairGrowthState);
                    }
                }
            }
            simDescription.mSpecialOutfitIndices.Clear();
            foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in specialOutfitIndices)
            {
                simDescription.mSpecialOutfitIndices.Add(specialOutfitIndexKvp.Key, specialOutfitIndexKvp.Value);
            }
            if (simDescription.CreatedSim != null)
            {
                ((Sims3.Gameplay.UI.HudModel)Sims3.UI.Responder.Instance.HudModel).NotifySimChanged(simDescription.CreatedSim.ObjectId);
            }
            simBuilder.Dispose();
        }

        public static void ApplyHairstyleWithGrowthStateToOutfit(this SimDescription simDescription, SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex, HairGrowthStates hairGrowthState)
        {
            HairGrowthStates outfitHairGrowthState;
            if (simDescription.GetOutfit(outfitCategory, outfitIndex).TryGetHairGrowthState(out outfitHairGrowthState) && (outfitHairGrowthState & hairGrowthState) != 0)
            {
                return;
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
                return;
            }
            simBuilder.Clear();
            OutfitUtils.SetAutomaticModifiers(simBuilder);
            OutfitUtils.SetOutfit(simBuilder, simDescription.GetOutfit(outfitCategory, outfitIndex), null);
            simBuilder.RemoveParts(BodyTypes.Hair);
            simBuilder.AddPart(validHairCASPs[Sims3.Gameplay.Core.RandomUtil.GetInt(0, validHairCASPs.Count - 1)]);
            OutfitUtils.InjectBodyHairColor(simBuilder, simDescription.BodyHairColor.ActiveColor);
            OutfitUtils.InjectEyeBrowHairColor(simBuilder, simDescription.EyebrowColor.ActiveColor);
            OutfitUtils.InjectHairColor(simBuilder, Array.ConvertAll(simDescription.FacialHairColors, x => x.ActiveColor), BodyTypes.Beard);
            OutfitUtils.InjectHairColor(simBuilder, Array.ConvertAll(simDescription.HairColors, x => x.ActiveColor), BodyTypes.Hair);
            SimOutfit outfit = new SimOutfit(simBuilder.CacheOutfit(string.Format("Rebuilt_{0}_{1}", outfitCategory, outfitIndex)));
            if (outfitCategory == OutfitCategories.Special)
            {
                uint key = simDescription.GetSpecialOutfitKeyForIndex(outfitIndex);
                simDescription.RemoveSpecialOutfit(key);
                simDescription.AddSpecialOutfit(outfit, key);
            }
            else
            {
                simDescription.RemoveOutfit(outfitCategory, outfitIndex, true);
                simDescription.AddOutfit(outfit, outfitCategory, outfitIndex);
            }
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

        public static void OnHairGrowthStateChanged(this SimDescription simDescription, HairGrowthStates hairGrowthState, HairGrowthStateChangeFlags flags)
        {
            // Change hairstyles of all outfits to those of the corresponding growth states
            simDescription.ApplyHairstylesWithGrowthStateToAllOutfits(hairGrowthState);
            // If dyed hair is grown out naturally (not via cheats), enable showing the roots of the original hair color
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
