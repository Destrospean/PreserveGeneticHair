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
            simDescription.BodyHairColor = bodyHairColor;
            simDescription.EyebrowColor = eyebrowColor;
            simDescription.FacialHairColors = facialHairColors;
            simDescription.HairColors = hairColors;
            simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) => ApplyOverallHairColorsToOutfit(simDescription, simBuilder, outfitCategory, outfitIndex, bodyHairColor, eyebrowColor, facialHairColors, hairColors), spin);
        }

        public static SimOutfit ApplyOverallHairColorsToOutfit(this SimDescription simDescription, SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex, GeneticColor bodyHairColor, GeneticColor eyebrowColor, GeneticColor[] facialHairColors, GeneticColor[] hairColors)
        {
            simBuilder.PrepareForOutfit(simDescription.GetOutfit(outfitCategory, outfitIndex));
            OutfitUtils.InjectBodyHairColor(simBuilder, bodyHairColor.ActiveColor);
            OutfitUtils.InjectEyeBrowHairColor(simBuilder, eyebrowColor.ActiveColor);
            OutfitUtils.InjectHairColor(simBuilder, System.Array.ConvertAll(facialHairColors, x => x.ActiveColor), BodyTypes.Beard);
            OutfitUtils.InjectHairColor(simBuilder, System.Array.ConvertAll(hairColors, x => x.ActiveColor), BodyTypes.Hair);
            return new SimOutfit(simBuilder.CacheOutfit(string.Format("Rebuilt_{0}_{1}", outfitCategory, outfitIndex)));
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
