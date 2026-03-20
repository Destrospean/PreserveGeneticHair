using System;
using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Objects.Decorations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExtensionAttribute : Attribute
    {
    }
}

namespace Destrospean.PreserveGeneticHair
{
    [Plugin]
    public class Main
    {
        static EventListener sSimDescriptionDisposedListener, sSimInstantiatedListener, sSimSelectedListener;

        static Main()
        {
            SimHairGrowth.StateChanged += (sender, e) =>
                {
                    foreach (OutfitCategories outfitCategory in e.SimDescription.ListOfCategories)
                    {
                        for (int i = 0; i < e.SimDescription.GetOutfitCount(outfitCategory); i++)
                        {
                            HairGrowthStates hairGrowthState;
                            if (SimHairGrowth.StateCASPartKeyMap.TryGetValue(Array.Find(e.SimDescription.GetOutfit(outfitCategory, i).Parts, x => x.BodyType == BodyTypes.Hair).Key.ToString(), out hairGrowthState) && hairGrowthState > e.SimDescription.GetHairGrowthState())
                            {
                                // <code to change hairstyle to one of the right growth state here>
                            }
                        }
                    }
                    int tempIndex = 0;
                    if (!e.SimDescription.HasRootsShowing() && (e.Flags & HairGrowthStateChangeFlags.NaturalGrowth) != 0 && Array.Exists(e.SimDescription.GetOriginalHairColors(), x => x.ActiveColor != e.SimDescription.HairColors[tempIndex++].ActiveColor))
                    {
                        e.SimDescription.HasRootsShowing(true);
                    }
                };
            World.sOnObjectPlacedInLotEventHandler += (sender, e) =>
                {
                    World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
                    if (onObjectPlacedInLotEventArgs != null)
                    {
                        AddInteractions(Sims3.Gameplay.Abstracts.GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId) as Mirror);
                    }
                };
            World.sOnWorldLoadFinishedEventHandler += (sender, e) =>
                {
                    foreach (Mirror mirror in Sims3.Gameplay.Queries.GetObjects<Mirror>())
                    {
                        AddInteractions(mirror);
                    }
                    foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                    {
                        AddInteractions(sim);
                    }
                    sSimDescriptionDisposedListener = EventTracker.AddListener(EventTypeId.kSimDescriptionDisposed, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    sim.SimDescription.ClearOriginalHairColors();
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    sSimInstantiatedListener = EventTracker.AddListener(EventTypeId.kSimInstantiated, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    AddInteractions(sim);
                                    sim.SimDescription.InitOriginalHairColors();
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    sSimSelectedListener = EventTracker.AddListener(EventTypeId.kEventSimSelected, evt =>
                        {
                            try
                            {
                                if (SimHairData.OriginalHairColors.Count == 0)
                                {
                                    foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                                    {
                                        sim.SimDescription.InitOriginalHairColors();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                };
            World.sOnWorldQuitEventHandler += (sender, e) =>
                {
                    EventTracker.RemoveListener(sSimDescriptionDisposedListener);
                    EventTracker.RemoveListener(sSimInstantiatedListener);
                    EventTracker.RemoveListener(sSimSelectedListener);
                    sSimDescriptionDisposedListener = null;
                    sSimInstantiatedListener = null;
                    sSimSelectedListener = null;
                };
        }

        static void AddInteractions(Mirror mirror)
        {
            if (!mirror.Interactions.Exists(interaction => interaction.InteractionDefinition.GetType() == Interactions.RemoveHairDye.Singleton.GetType()))
            {
                mirror.AddInteraction(Interactions.RemoveHairDye.Singleton);
            }
        }

        static void AddInteractions(Sim sim)
        {
            if (!sim.Interactions.Exists(interaction => interaction.InteractionDefinition.GetType() == Interactions.ResetOriginalHair.Singleton.GetType()))
            {
                sim.AddInteraction(Interactions.ResetOriginalHair.Singleton);
            }
        }
    }
}
