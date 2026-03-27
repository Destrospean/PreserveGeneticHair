using System;
using Destrospean.PreserveGeneticHair;
using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
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

namespace Destrospean.HairTrouble
{
    [Plugin]
    public static class Main
    {
        static EventListener sSimDescriptionDisposedListener, sSimInstantiatedListener, sSimSelectedListener;

        static Main()
        {
            SimHairGrowth.HairGrowthStateChanged += (sender, e) =>
                {
                    try
                    {
                        // Change hairstyles of all outfits to those of the corresponding growth states
                        e.SimDescription.ApplyHairstylesWithGrowthStateToAllOutfits(e.HairGrowthState);
                        // If dyed hair is grown out naturally (not via cheats), enable showing the roots of the original hair color
                        if (!e.SimDescription.HasRootsShowing() && (e.Flags & HairGrowthStateChangeFlags.NaturalGrowth) != 0 && !e.SimDescription.HasOriginalHairColors())
                        {
                            e.SimDescription.HasRootsShowing(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
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
                                    sim.SimDescription.ClearOriginalOverallHairColors();
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
                                    sim.SimDescription.InitOriginalOverallHairColors();
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
                                        sim.SimDescription.InitOriginalOverallHairColors();
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

        public static ResourceKey FromS3PIFormatKeyString(string key)
        {
            string[] tgi = key.Replace("0x", "").Split('-');
            return new ResourceKey(Convert.ToUInt64(tgi[2], 16), Convert.ToUInt32(tgi[0], 16), Convert.ToUInt32(tgi[1], 16));
        }

        public static string ToS3PIFormatKeyString(this ResourceKey key)
        {
            return string.Format("0x{0:X8}-0x{1:X8}-0x{2:X16}", key.TypeId, key.GroupId, key.InstanceId);
        }
    }
}
