using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;

namespace Destrospean.HairTrouble
{
    public static class Common
    {
        public delegate SimOutfit OutfitFunc(SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex);

        public static void ApplyToAllOutfits(this SimDescription simDescription, OutfitFunc outfitFunc, bool spin = false)
        {
            SimBuilder simBuilder = new SimBuilder
                {
                    UseCompression = true
                };
            OutfitCategories lastOutfitCategory = 0,
            tempOutfitCategory = OutfitCategories.Everyday;
            int lastOutfitIndex = 0,
            tempOutfitIndex = simDescription.GetOutfitCount(tempOutfitCategory);
            if (simDescription.CreatedSim != null)
            {
                lastOutfitCategory = simDescription.CreatedSim.CurrentOutfitCategory;
                lastOutfitIndex = simDescription.CreatedSim.CurrentOutfitIndex;
                if (spin)
                {
                    simDescription.AddOutfit(new SimOutfit(simDescription.CreatedSim.CurrentOutfit.Key), tempOutfitCategory);
                    simDescription.CreatedSim.SwitchToOutfitWithoutSpin(tempOutfitCategory, tempOutfitIndex);
                    simDescription.ReplaceOutfit(lastOutfitCategory, lastOutfitIndex, outfitFunc(simBuilder, lastOutfitCategory, lastOutfitIndex));
                    using (Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper switchOutfitHelper = new Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper(simDescription.CreatedSim, Sims3.Gameplay.Actors.Sim.ClothesChangeReason.Force, lastOutfitCategory, lastOutfitIndex, false))
                    {
                        simDescription.CreatedSim.SwitchToOutfitWithSpin(switchOutfitHelper);
                    }
                    simDescription.RemoveOutfit(tempOutfitCategory, tempOutfitIndex, true);
                }
            }
            Dictionary<uint, int> specialOutfitIndices = new Dictionary<uint, int>();
            if (simDescription.mSpecialOutfitIndices != null)
            {
                foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in simDescription.mSpecialOutfitIndices)
                {
                    specialOutfitIndices.Add(specialOutfitIndexKvp.Key, simDescription.mSpecialOutfitIndices.Count - 1 - specialOutfitIndexKvp.Value);
                }
            }
            foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
            {
                for (int i = simDescription.GetOutfitCount(outfitCategory) - 1; i > -1 ; i--)
                {
                    if (simDescription.CreatedSim == null || outfitCategory != lastOutfitCategory || i != lastOutfitIndex || !spin)
                    {
                        simDescription.ReplaceOutfit(outfitCategory, i, outfitFunc(simBuilder, outfitCategory, i));
                    }
                }
            }
            if (simDescription.mSpecialOutfitIndices != null)
            {
                simDescription.mSpecialOutfitIndices.Clear();
                foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in specialOutfitIndices)
                {
                    simDescription.mSpecialOutfitIndices.Add(specialOutfitIndexKvp.Key, specialOutfitIndexKvp.Value);
                }
            }
            if (simDescription.CreatedSim != null)
            {
                ((Sims3.Gameplay.UI.HudModel)Sims3.UI.Responder.Instance.HudModel).NotifySimChanged(simDescription.CreatedSim.ObjectId);
                if (!spin)
                {
                    simDescription.CreatedSim.RefreshCurrentOutfit(false);
                }
            }
            simBuilder.Dispose();
        }

        public static ResourceKey FromS3PIFormatKeyString(string key)
        {
            string[] tgi = key.Replace("0x", "").Split('-');
            return new ResourceKey(System.Convert.ToUInt64(tgi[2], 16), System.Convert.ToUInt32(tgi[0], 16), System.Convert.ToUInt32(tgi[1], 16));
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

        public static void PrepareForOutfit(this SimBuilder simBuilder, SimOutfit outfit)
        {
            simBuilder.Clear();
            OutfitUtils.SetAutomaticModifiers(simBuilder);
            OutfitUtils.SetOutfit(simBuilder, outfit, null);
        }

        public static void ReplaceOutfit(this SimDescription simDescription, OutfitCategories outfitCategory, int outfitIndex, SimOutfit newOutfit)
        {
            if (newOutfit != null)
            {
                if (outfitCategory == OutfitCategories.Special)
                {
                    uint key = simDescription.GetSpecialOutfitKeyForIndex(outfitIndex);
                    simDescription.RemoveSpecialOutfit(key);
                    simDescription.AddSpecialOutfit(newOutfit, key);
                }
                else
                {
                    simDescription.RemoveOutfit(outfitCategory, outfitIndex, true);
                    simDescription.AddOutfit(newOutfit, outfitCategory, outfitIndex);
                }
            }
        }

        public static string ToS3PIFormatKeyString(this ResourceKey key)
        {
            return string.Format("0x{0:X8}-0x{1:X8}-0x{2:X16}", key.TypeId, key.GroupId, key.InstanceId);
        }
    }
}
