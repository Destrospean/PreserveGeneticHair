using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;

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

        public static GeneticColor GetOriginalBodyHairColor(this SimDescription simDescription)
        {
            GeneticColor originalColor;
            return OriginalBodyHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : (OriginalBodyHairColors[simDescription.SimDescriptionId] = simDescription.BodyHairColor);
        }

        public static GeneticColor GetOriginalEyebrowColor(this SimDescription simDescription)
        {
            GeneticColor originalColor;
            return OriginalEyebrowColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : (OriginalEyebrowColors[simDescription.SimDescriptionId] = simDescription.EyebrowColor);
        }

        public static GeneticColor[] GetOriginalFacialHairColors(this SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return OriginalFacialHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : (OriginalFacialHairColors[simDescription.SimDescriptionId] = simDescription.FacialHairColors);
        }

        public static GeneticColor[] GetOriginalHairColors(this SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return OriginalHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : (OriginalHairColors[simDescription.SimDescriptionId] = simDescription.HairColors);
        }
    }
}
