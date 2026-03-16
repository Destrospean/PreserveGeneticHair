using System;
using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;

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
    public class PreserveGeneticHair
    {
        static EventListener sSimDescriptionDisposedListener, sSimInstantiatedListener, sSimSelectedListener;

        static PreserveGeneticHair()
        {
            World.sOnWorldLoadFinishedEventHandler += (sender, e) =>
                {
                    sSimDescriptionDisposedListener = EventTracker.AddListener(EventTypeId.kSimDescriptionDisposed, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    SimHairData.OriginalBodyHairColors.Remove(sim.SimDescription.SimDescriptionId);
                                    SimHairData.OriginalEyebrowColors.Remove(sim.SimDescription.SimDescriptionId);
                                    SimHairData.OriginalFacialHairColors.Remove(sim.SimDescription.SimDescriptionId);
                                    SimHairData.OriginalHairColors.Remove(sim.SimDescription.SimDescriptionId);
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
                                    sim.SimDescription.GetOriginalBodyHairColor();
                                    sim.SimDescription.GetOriginalEyebrowColor();
                                    sim.SimDescription.GetOriginalFacialHairColors();
                                    sim.SimDescription.GetOriginalHairColors();
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
                                        sim.SimDescription.GetOriginalBodyHairColor();
                                        sim.SimDescription.GetOriginalEyebrowColor();
                                        sim.SimDescription.GetOriginalFacialHairColors();
                                        sim.SimDescription.GetOriginalHairColors();
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
                    sSimInstantiatedListener = null;
                    sSimDescriptionDisposedListener = null;
                    sSimSelectedListener = null;
                };
        }

        [ReplaceMethod(typeof(Genetics), "InheritHairColor")]
        public static Color[] InheritHairColor(Sims3.SimIFace.CAS.SimBuilder target, SimDescription[] parentSims, Random rnd)
        {
            List<Genealogy> genealogies = new List<Genealogy>();
            foreach (SimDescription parentSim in parentSims)
            {
                if (parentSim.Genealogy != null)
                {
                    genealogies.Add(parentSim.Genealogy);
                }
            }
            if (genealogies.Count == 0)
            {
                return Genetics.GeneticColors[rnd.Next(Genetics.GeneticColors.Length)] as Color[];
            }
            if (genealogies.Count == 1)
            {
                genealogies.Add(genealogies[0]);
            }
            List<Genealogy> parentGenealogies = new List<Genealogy>();
            foreach (Genealogy genealogy in genealogies)
            {
                foreach (Genealogy parent in genealogy.Parents)
                {
                    if (parent.SimDescription != null)
                    {
                        parentGenealogies.Add(parent);
                    }
                }
            }
            if ((float)rnd.NextDouble() * 100 < Genetics.kMutateHairColorChance)
            {
                return Genetics.GeneticColors[rnd.Next(Genetics.GeneticColors.Length)] as Color[];
            }
            SimDescription simDescription;
            if (parentGenealogies.Count > 0 && (float)rnd.NextDouble() * 100 < Genetics.kHairColorChooseGrandparentChance)
            {
                simDescription = parentGenealogies[rnd.Next(0, parentGenealogies.Count)].SimDescription;
            }
            else
            {
                simDescription = genealogies[rnd.Next(0, genealogies.Count)].SimDescription;
            }
            Color[] colors = new Color[10];
            for (int i = 0; i < 4; i++)
            {
                colors[i] = simDescription.GetOriginalHairColors()[i].Genetic;
            }
            colors[4] = simDescription.GetOriginalEyebrowColor().Genetic;
            for (int i = 0; i < 4; i++)
            {
                colors[i + 5] = simDescription.GetOriginalFacialHairColors()[i].Genetic;
            }
            colors[9] = simDescription.GetOriginalBodyHairColor().Genetic;
            return colors;
        }
    }
}
