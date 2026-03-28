using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.UI;

namespace Destrospean.HairTrouble
{
    public static class Common
    {
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

        public static string ToS3PIFormatKeyString(this ResourceKey key)
        {
            return string.Format("0x{0:X8}-0x{1:X8}-0x{2:X16}", key.TypeId, key.GroupId, key.InstanceId);
        }
    }
}
