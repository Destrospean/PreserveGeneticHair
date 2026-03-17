using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects.Decorations;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public class Interactions
    {
        const string kLocalizationPath = "Destrospean/PreserveGeneticHair/Interactions";

        public class RemoveHairDye : Interaction<Sim, IMirror>
        {
            public class Definition : InteractionDefinition<Sim, IMirror, RemoveHairDye>
            {
                public override string GetInteractionName(Sim actor, IMirror target, Sims3.Gameplay.Autonomy.InteractionObjectPair interaction)
                {
                    return Localization.LocalizeString(actor.IsFemale, kLocalizationPath + "/RemoveHairDye:Name");
                }

                public override bool Test(Sim actor, IMirror target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (actor.CurrentOutfitCategory == OutfitCategories.Singed || actor.SimDescription.IsUsingMaternityOutfits || actor.SimDescription.Age == CASAgeGenderFlags.Baby || actor.OccultManager.DisallowClothesChange())
                    {
                        return false;
                    }
                    if (actor.BuffManager.HasTransformBuff())
                    {
                        greyedOutTooltipCallback = InteractionInstance.CreateTooltipCallback(Localization.LocalizeString(actor.IsFemale, "Gameplay/Buffs/BuffTransformation:CantChangeClothesTooltip", actor));
                        return false;
                    }
                    return true;
                }
            }

            public static InteractionDefinition Singleton = new Definition();

            public static void ApplyGeneticHairColorsToOutfit(SimDescription simDescription, OutfitCategories outfitCategory, int outfitIndex)
            {
                SimBuilder simBuilder = new SimBuilder
                    {
                        UseCompression = true
                    };
                simBuilder.Clear();
                OutfitUtils.SetAutomaticModifiers(simBuilder);
                OutfitUtils.SetOutfit(simBuilder, (SimOutfit)simDescription.GetOutfit(outfitCategory, outfitIndex), null);
                OutfitUtils.InjectBodyHairColor(simBuilder, simDescription.GetOriginalBodyHairColor().ActiveColor);
                OutfitUtils.InjectEyeBrowHairColor(simBuilder, simDescription.GetOriginalEyebrowColor().ActiveColor);
                Color[] hairColors = System.Array.ConvertAll(simDescription.GetOriginalHairColors(), x => x.ActiveColor);
                OutfitUtils.InjectHairColor(simBuilder, hairColors, BodyTypes.Beard);
                OutfitUtils.InjectHairColor(simBuilder, hairColors, BodyTypes.Hair);
                SimOutfit outfit = new SimOutfit(simBuilder.CacheOutfit(string.Format("Rebuilt_{0}_{1}", outfitCategory, outfitIndex)));
                if (outfitCategory == OutfitCategories.Special)
                {
                    uint specialOutfitKeyForIndex = simDescription.GetSpecialOutfitKeyForIndex(outfitIndex);
                    simDescription.RemoveSpecialOutfitAtIndex(outfitIndex);
                    simDescription.AddSpecialOutfit(outfit, specialOutfitKeyForIndex);
                }
                else
                {
                    simDescription.RemoveOutfit(outfitCategory, outfitIndex, true);
                    simDescription.AddOutfit(outfit, outfitCategory, outfitIndex);
                }
            }

            public static void ResetHairColorsToGenetic(SimDescription simDescription)
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
                }
                ApplyGeneticHairColorsToOutfit(simDescription, lastOutfitCategory, lastOutfitIndex);
                if (simDescription.CreatedSim != null)
                {
                    simDescription.CreatedSim.SwitchToOutfitWithSpin(lastOutfitCategory, lastOutfitIndex);
                    simDescription.RemoveOutfit(OutfitCategories.Everyday, tempOutfitIndex, true);
                }
                Tasks.TaskGenericAction.Start(() =>
                    {
                        foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
                        {
                            if (outfitCategory == OutfitCategories.Supernatural)
                            {
                                continue;
                            }
                            for (int i = 0; i < simDescription.GetOutfitCount(outfitCategory); i++)
                            {
                                if (outfitCategory == lastOutfitCategory && i == lastOutfitIndex)
                                {
                                    continue;
                                }
                                ApplyGeneticHairColorsToOutfit(simDescription, outfitCategory, i);
                            }
                        }
                    });
            }

            public override bool Run()
            {
                if (!Target.Route(this))
                {
                    return false;
                }
                StandardEntry();
                Mirror.EnterStateMachine((Interaction<Sim, IMirror>)this);
                AnimateSim("ChangeAppearance");
                if (!Actor.IsSelectable)
                {
                    StandardExit();
                    return false;
                }
                ResetHairColorsToGenetic(Actor.SimDescription);
                StandardExit();
                Sims3.Gameplay.UI.HudModel hudModel = Sims3.UI.Responder.Instance.HudModel as Sims3.Gameplay.UI.HudModel;
                hudModel.NotifySimChanged(Actor.ObjectId);
                return true;
            }
        }
    }
}
