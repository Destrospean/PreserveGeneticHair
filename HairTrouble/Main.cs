using System;
using Destrospean.PreserveGeneticHair;
using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Objects.Decorations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;

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
    public class Main
    {
        static EventListener sSimDescriptionDisposedListener, sSimInstantiatedListener, sSimSelectedListener;

        public static object Lock = new object();

        static Main()
        {
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
            if (!sim.Interactions.Exists(interaction => interaction.InteractionDefinition.GetType() == Interactions.DecrementHairGrowthState.Singleton.GetType()))
            {
                sim.AddInteraction(Interactions.DecrementHairGrowthState.Singleton);
                sim.AddInteraction(Interactions.IncrementHairGrowthState.Singleton);
                sim.AddInteraction(Interactions.ResetOriginalHair.Singleton);
            }
        }

        public static ResourceKey FromS3PIFormatKeyString(string key)
        {
            string[] tgi = key.Replace("0x", "").Split('-');
            return new ResourceKey(Convert.ToUInt64(tgi[2], 16), Convert.ToUInt32(tgi[0], 16), Convert.ToUInt32(tgi[1], 16));
        }

        public static void Notify(string message, SimDescription simDescription, StyledNotification.NotificationStyle style)
        {
            Notify(message, simDescription, style, true);
        }

        public static void Notify(string message, SimDescription fakeSimDescription, StyledNotification.NotificationStyle style, bool checkForFake)
        {
            SimDescription simDescription = fakeSimDescription;
            if (simDescription == null)
            {
                StyledNotification.Show(new StyledNotification.Format(message, style));
                return;
            }
            if (checkForFake)
            {
                simDescription = SimDescription.Find(fakeSimDescription.SimDescriptionId);
                if (simDescription == null)
                {
                    StyledNotification.Show(new StyledNotification.Format(message, style));
                    return;
                }
            }
            if (simDescription.CreatedSim != null)
            {
                StyledNotification.Show(new StyledNotification.Format(message, ObjectGuid.InvalidObjectGuid, simDescription.CreatedSim.ObjectId, style));
            }
            else
            {
                StyledNotification.Show(new StyledNotification.Format(message, style));
            }
        }

        public static string ToS3PIFormatKeyString(ResourceKey key)
        {
            return string.Format("0x{0:X8}-0x{1:X8}-0x{2:X16}", key.TypeId, key.GroupId, key.InstanceId);
        }
    }
}
