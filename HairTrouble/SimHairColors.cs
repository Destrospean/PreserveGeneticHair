using System.Collections.Generic;
using Destrospean.PreserveGeneticHair;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;

namespace Destrospean.HairTrouble
{
    public static class SimHairColors
    {
        public enum Channels
        {
            Base,
            Highlight,
            Root,
            Tip
        }

        public static void ApplyOverallHairColorsToAllOutfits(this SimDescription simDescription, GeneticColor bodyHairColor, GeneticColor eyebrowColor, GeneticColor[] facialHairColors, GeneticColor[] hairColors, bool spin = false)
        {
            SimBuilder simBuilder = new SimBuilder
                {
                    UseCompression = true
                };
            simDescription.BodyHairColor = simDescription.GetOriginalBodyHairColor();
            simDescription.EyebrowColor = simDescription.GetOriginalEyebrowColor();
            simDescription.FacialHairColors = simDescription.GetOriginalFacialHairColors();
            simDescription.HairColors = simDescription.GetOriginalHairColors();
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
                ApplyOverallHairColorsToOutfit(simDescription, simBuilder, lastOutfitCategory, lastOutfitIndex, bodyHairColor, eyebrowColor, facialHairColors, hairColors);
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
                        ApplyOverallHairColorsToOutfit(simDescription, simBuilder, outfitCategory, i, bodyHairColor, eyebrowColor, facialHairColors, hairColors);
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

        public static void ApplyOverallHairColorsToOutfit(this SimDescription simDescription, SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex, GeneticColor bodyHairColor, GeneticColor eyebrowColor, GeneticColor[] facialHairColors, GeneticColor[] hairColors)
        {
            simBuilder.Clear();
            OutfitUtils.SetAutomaticModifiers(simBuilder);
            OutfitUtils.SetOutfit(simBuilder, simDescription.GetOutfit(outfitCategory, outfitIndex), null);
            OutfitUtils.InjectBodyHairColor(simBuilder, bodyHairColor.ActiveColor);
            OutfitUtils.InjectEyeBrowHairColor(simBuilder, eyebrowColor.ActiveColor);
            OutfitUtils.InjectHairColor(simBuilder, System.Array.ConvertAll(facialHairColors, x => x.ActiveColor), BodyTypes.Beard);
            OutfitUtils.InjectHairColor(simBuilder, System.Array.ConvertAll(hairColors, x => x.ActiveColor), BodyTypes.Hair);
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

        public static void ClearOriginalOverallHairColors(this SimDescription simDescription)
        {
            SimHairData.OriginalBodyHairColors.Remove(simDescription.SimDescriptionId);
            SimHairData.OriginalEyebrowColors.Remove(simDescription.SimDescriptionId);
            SimHairData.OriginalFacialHairColors.Remove(simDescription.SimDescriptionId);
            SimHairData.OriginalHairColors.Remove(simDescription.SimDescriptionId);
        }

        public static GeneticColor GetOriginalBodyHairColor(this SimDescription simDescription)
        {
            GeneticColor originalColor;
            return SimHairData.OriginalBodyHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : SimHairData.OriginalBodyHairColors[simDescription.SimDescriptionId] = simDescription.BodyHairColor;
        }

        public static GeneticColor GetOriginalEyebrowColor(this SimDescription simDescription)
        {
            GeneticColor originalColor;
            return SimHairData.OriginalEyebrowColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : SimHairData.OriginalEyebrowColors[simDescription.SimDescriptionId] = simDescription.EyebrowColor;
        }

        public static GeneticColor[] GetOriginalFacialHairColors(this SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return SimHairData.OriginalFacialHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : SimHairData.OriginalFacialHairColors[simDescription.SimDescriptionId] = simDescription.FacialHairColors;
        }

        public static GeneticColor[] GetOriginalHairColors(this SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return SimHairData.OriginalHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : SimHairData.OriginalHairColors[simDescription.SimDescriptionId] = simDescription.HairColors;
        }

        public static bool HasOriginalBodyHairColor(this SimDescription simDescription)
        {
            return simDescription.GetOriginalBodyHairColor().ActiveColor == simDescription.BodyHairColor.ActiveColor;
        }

        public static bool HasOriginalEyebrowColor(this SimDescription simDescription)
        {
            return simDescription.GetOriginalEyebrowColor().ActiveColor == simDescription.EyebrowColor.ActiveColor;
        }

        public static bool HasOriginalFacialHairColors(this SimDescription simDescription)
        {
            int tempIndex = 0;
            return System.Array.TrueForAll(simDescription.GetOriginalFacialHairColors(), x => x.ActiveColor == simDescription.FacialHairColors[tempIndex++].ActiveColor);
        }

        public static bool HasOriginalHairColors(this SimDescription simDescription)
        {
            int tempIndex = 0;
            return System.Array.TrueForAll(simDescription.GetOriginalHairColors(), x => x.ActiveColor == simDescription.HairColors[tempIndex++].ActiveColor);
        }

        public static bool HasOriginalOverallHairColors(this SimDescription simDescription)
        {
            return simDescription.HasOriginalBodyHairColor() && simDescription.HasOriginalEyebrowColor() && simDescription.HasOriginalFacialHairColors() && simDescription.HasOriginalHairColors();
        }

        public static bool HasRootsShowing(this SimDescription simDescription, bool? value = null)
        {
            int rootIndex = (int)Channels.Root;
            if (value == null)
            {
                bool rootsShowing;
                if (!simDescription.HasOriginalHairColors() && SimHairData.RootsShowing.TryGetValue(simDescription.SimDescriptionId, out rootsShowing))
                {
                    return rootsShowing;
                }
                simDescription.HairColors[rootIndex].ActiveColor = simDescription.GetOriginalHairColors()[rootIndex].ActiveColor; 
                simDescription.ApplyOverallHairColorsToAllOutfits(simDescription.BodyHairColor, simDescription.EyebrowColor, simDescription.FacialHairColors, simDescription.HairColors);
                return SimHairData.RootsShowing[simDescription.SimDescriptionId] = false;
            }
            if (value.Value)
            {
                simDescription.HairColors[rootIndex].ActiveColor = simDescription.GetOriginalHairColors()[rootIndex].ActiveColor; 
                simDescription.ApplyOverallHairColorsToAllOutfits(simDescription.BodyHairColor, simDescription.EyebrowColor, simDescription.FacialHairColors, simDescription.HairColors);
            }
            return SimHairData.RootsShowing[simDescription.SimDescriptionId] = value.Value;
        }

        public static void InitOriginalOverallHairColors(this SimDescription simDescription)
        {
            simDescription.GetOriginalBodyHairColor();
            simDescription.GetOriginalEyebrowColor();
            simDescription.GetOriginalFacialHairColors();
            simDescription.GetOriginalHairColors();
        }
    }
}
