using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.Destrospean.HairTrouble;

namespace Destrospean.HairTrouble
{
    public class Interactions
    {
        const string kLocalizationPath = "Destrospean/HairTrouble/Interactions";

        public class DecrementHairGrowthState : ImmediateInteraction<Sim, Sim>
        {
            const string kLocalizationKey = kLocalizationPath + "/DecrementHairGrowthState";

            public static InteractionDefinition Singleton = new Definition();

            [DoesntRequireTuning]
            public class Definition : ImmediateInteractionDefinition<Sim, Sim, DecrementHairGrowthState>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(target.IsFemale, kLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Localization.LocalizeString(isFemale, kLocalizationPath + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kShowCheatInteractions;
                }
            }

            public override bool Run()
            {
                Actor.SimDescription.DecrementHairGrowthState();
                return true;
            }
        }

        public class IncrementHairGrowthState : ImmediateInteraction<Sim, Sim>
        {
            const string kLocalizationKey = kLocalizationPath + "/IncrementHairGrowthState";

            public static InteractionDefinition Singleton = new Definition();

            [DoesntRequireTuning]
            public class Definition : ImmediateInteractionDefinition<Sim, Sim, IncrementHairGrowthState>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(target.IsFemale, kLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Localization.LocalizeString(isFemale, kLocalizationPath + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kShowCheatInteractions;
                }
            }

            public override bool Run()
            {
                Actor.SimDescription.IncrementHairGrowthState();
                return true;
            }
        }

        public class RemoveHairDye : Interaction<Sim, IMirror>
        {
            const string kLocalizationKey = kLocalizationPath + "/RemoveHairDye";

            public static InteractionDefinition Singleton = new Definition();

            public class Definition : InteractionDefinition<Sim, IMirror, RemoveHairDye>
            {
                public override string GetInteractionName(Sim actor, IMirror target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(actor.IsFemale, kLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Localization.LocalizeString(isFemale, kLocalizationPath + ":Path")
                    };
                }

                public override bool Test(Sim actor, IMirror target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (actor.SimDescription.HasOriginalOverallHairColors() || actor.CurrentOutfitCategory == OutfitCategories.Singed || actor.SimDescription.Age == CASAgeGenderFlags.Baby || actor.OccultManager.DisallowClothesChange())
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
                Actor.SimDescription.ApplyOverallHairColorsToAllOutfits(Actor.SimDescription.GetOriginalBodyHairColor(), Actor.SimDescription.GetOriginalEyebrowColor(), Actor.SimDescription.GetOriginalFacialHairColors(), Actor.SimDescription.GetOriginalHairColors(), true);
                StandardExit();
                return true;
            }
        }

        public class ResetOriginalHair : ImmediateInteraction<Sim, Sim>
        {
            const string kLocalizationKey = kLocalizationPath + "/ResetOriginalHair";

            public static InteractionDefinition Singleton = new Definition();

            [DoesntRequireTuning]
            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ResetOriginalHair>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(target.IsFemale, kLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Localization.LocalizeString(isFemale, kLocalizationPath + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kShowCheatInteractions;
                }
            }

            public override bool Run()
            {
                Actor.SimDescription.ClearOriginalOverallHairColors();
                Actor.SimDescription.InitOriginalOverallHairColors();
                return true;
            }
        }
    }
}
