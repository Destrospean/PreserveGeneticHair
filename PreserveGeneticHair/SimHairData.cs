using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public static class SimHairData
    {
        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor> OriginalBodyHairColors = new Dictionary<ulong, GeneticColor>();

        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor> OriginalEyebrowColors = new Dictionary<ulong, GeneticColor>();

        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor[]> OriginalFacialHairColors = new Dictionary<ulong, GeneticColor[]>();

        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor[]> OriginalHairColors = new Dictionary<ulong, GeneticColor[]>();

        public static void ApplyOriginalHairColorsToAllOutfits(this SimDescription simDescription)
        {
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
                ApplyOriginalHairColorsToOutfit(simDescription, lastOutfitCategory, lastOutfitIndex);
                using (Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper switchOutfitHelper = new Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper(simDescription.CreatedSim, Sims3.Gameplay.Actors.Sim.ClothesChangeReason.Force, lastOutfitCategory, lastOutfitIndex, false))
                {
                    simDescription.CreatedSim.SwitchToOutfitWithSpin(switchOutfitHelper);
                }
                simDescription.RemoveOutfit(OutfitCategories.Everyday, tempOutfitIndex, true);
            }
            Tasks.TaskGenericAction.Start(() =>
                {
                    Dictionary<uint, int> specialOutfitIndices = new Dictionary<uint, int>();
                    foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in simDescription.mSpecialOutfitIndices)
                    {
                        specialOutfitIndices.Add(specialOutfitIndexKvp.Key, simDescription.mSpecialOutfitIndices.Count - 1 - specialOutfitIndexKvp.Value);
                    }
                    foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
                    {
                        for (int i = simDescription.GetOutfitCount(outfitCategory) - 1; i > -1 ; i--)
                        {
                            if (simDescription.CreatedSim != null && outfitCategory == lastOutfitCategory && i == lastOutfitIndex)
                            {
                                continue;
                            }
                            ApplyOriginalHairColorsToOutfit(simDescription, outfitCategory, i);
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
                });
        }

        public static void ApplyOriginalHairColorsToOutfit(this SimDescription simDescription, OutfitCategories outfitCategory, int outfitIndex)
        {
            using (SimBuilder simBuilder = new SimBuilder
                {
                    UseCompression = true
                })
            {
                simBuilder.Clear();
                OutfitUtils.SetAutomaticModifiers(simBuilder);
                OutfitUtils.SetOutfit(simBuilder, simDescription.GetOutfit(outfitCategory, outfitIndex), null);
                OutfitUtils.InjectBodyHairColor(simBuilder, simDescription.GetOriginalBodyHairColor().ActiveColor);
                OutfitUtils.InjectEyeBrowHairColor(simBuilder, simDescription.GetOriginalEyebrowColor().ActiveColor);
                OutfitUtils.InjectHairColor(simBuilder, System.Array.ConvertAll(simDescription.GetOriginalFacialHairColors(), x => x.ActiveColor), BodyTypes.Beard);
                OutfitUtils.InjectHairColor(simBuilder, System.Array.ConvertAll(simDescription.GetOriginalHairColors(), x => x.ActiveColor), BodyTypes.Hair);
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
        }

        public static void ClearOriginalHairColors(this SimDescription simDescription)
        {
            OriginalBodyHairColors.Remove(simDescription.SimDescriptionId);
            OriginalEyebrowColors.Remove(simDescription.SimDescriptionId);
            OriginalFacialHairColors.Remove(simDescription.SimDescriptionId);
            OriginalHairColors.Remove(simDescription.SimDescriptionId);
        }

        public static GeneticColor GetOriginalBodyHairColor(this SimDescription simDescription)
        {
            GeneticColor originalColor;
            return OriginalBodyHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : OriginalBodyHairColors[simDescription.SimDescriptionId] = simDescription.BodyHairColor;
        }

        public static GeneticColor GetOriginalEyebrowColor(this SimDescription simDescription)
        {
            GeneticColor originalColor;
            return OriginalEyebrowColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : OriginalEyebrowColors[simDescription.SimDescriptionId] = simDescription.EyebrowColor;
        }

        public static GeneticColor[] GetOriginalFacialHairColors(this SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return OriginalFacialHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : OriginalFacialHairColors[simDescription.SimDescriptionId] = simDescription.FacialHairColors;
        }

        public static GeneticColor[] GetOriginalHairColors(this SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return OriginalHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : OriginalHairColors[simDescription.SimDescriptionId] = simDescription.HairColors;
        }

        public static bool HasOriginalHairColors(this SimDescription simDescription)
        {
            int tempIndex = 0;
            return simDescription.GetOriginalBodyHairColor().ActiveColor == simDescription.BodyHairColor.ActiveColor && simDescription.GetOriginalEyebrowColor().ActiveColor == simDescription.EyebrowColor.ActiveColor && System.Array.TrueForAll(simDescription.GetOriginalFacialHairColors(), x => x.ActiveColor == simDescription.FacialHairColors[tempIndex++].ActiveColor) && System.Array.TrueForAll(simDescription.GetOriginalHairColors(), x => x.ActiveColor == simDescription.HairColors[tempIndex++ - simDescription.FacialHairColors.Length].ActiveColor);
        }

        public static void InitOriginalHairColors(this SimDescription simDescription)
        {
            simDescription.GetOriginalBodyHairColor();
            simDescription.GetOriginalEyebrowColor();
            simDescription.GetOriginalFacialHairColors();
            simDescription.GetOriginalHairColors();
        }
    }
}
