using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace.CAS;

namespace Destrospean.PreserveGeneticHair
{
    public class Interactions
    {
        const string kLocalizationPath = "Destrospean/PreserveGeneticHair/Interactions";

        public class RemoveHairDye : Interaction<Sim, IMirror>
        {
            public static InteractionDefinition Singleton = new Definition();

            public class Definition : InteractionDefinition<Sim, IMirror, RemoveHairDye>
            {
                public override string GetInteractionName(Sim actor, IMirror target, Sims3.Gameplay.Autonomy.InteractionObjectPair interaction)
                {
                    return Localization.LocalizeString(actor.IsFemale, kLocalizationPath + "/RemoveHairDye:Name");
                }

                public override bool Test(Sim actor, IMirror target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (actor.SimDescription.HasOriginalHairColors() || actor.CurrentOutfitCategory == OutfitCategories.Singed || actor.SimDescription.IsUsingMaternityOutfits || actor.SimDescription.Age == CASAgeGenderFlags.Baby || actor.OccultManager.DisallowClothesChange())
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

            public override bool Run()
            {
                if (!Target.Route(this))
                {
                    return false;
                }
                StandardEntry();
                Sims3.Gameplay.Objects.Decorations.Mirror.EnterStateMachine((Interaction<Sim, IMirror>)this);
                AnimateSim("ChangeAppearance");
                if (!Actor.IsSelectable)
                {
                    StandardExit();
                    return false;
                }
                Actor.SimDescription.ApplyOriginalHairColorsToAllOutfits();
                StandardExit();
                return true;
            }
        }
    }
}
