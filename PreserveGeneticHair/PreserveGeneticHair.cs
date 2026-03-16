using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public class PreserveGeneticHair
    {
        [Tunable]
        protected static bool kInstantiator;

        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor> OriginalEyebrowColors = new Dictionary<ulong, GeneticColor>();

        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor[]> OriginalFacialHairColors = new Dictionary<ulong, GeneticColor[]>();

        [PersistableStatic(true)]
        public static Dictionary<ulong, GeneticColor[]> OriginalHairColors = new Dictionary<ulong, GeneticColor[]>();

        public static GeneticColor GetOriginalEyebrowColor(SimDescription simDescription)
        {
            GeneticColor originalColor;
            return OriginalEyebrowColors.TryGetValue(simDescription.SimDescriptionId, out originalColor) ? originalColor : simDescription.EyebrowColor;
        }

        public static GeneticColor[] GetOriginalFacialHairColors(SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return OriginalFacialHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : simDescription.FacialHairColors;
        }

        public static GeneticColor[] GetOriginalHairColors(SimDescription simDescription)
        {
            GeneticColor[] originalColors;
            return OriginalHairColors.TryGetValue(simDescription.SimDescriptionId, out originalColors) ? originalColors : simDescription.HairColors;
        }

        public static void SetOriginalHairColors(SimDescription simDescription)
        {
            if (!OriginalEyebrowColors.ContainsKey(simDescription.SimDescriptionId))
            {
                OriginalEyebrowColors[simDescription.SimDescriptionId] = simDescription.EyebrowColor;
            }
            if (!OriginalFacialHairColors.ContainsKey(simDescription.SimDescriptionId))
            {
                OriginalFacialHairColors[simDescription.SimDescriptionId] = simDescription.FacialHairColors;
            }
            if (!OriginalHairColors.ContainsKey(simDescription.SimDescriptionId))
            {
                OriginalHairColors[simDescription.SimDescriptionId] = simDescription.HairColors;
            }
        }
    }
}
