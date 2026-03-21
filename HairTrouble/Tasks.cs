using Sims3.SimIFace;

namespace Destrospean.HairTrouble
{
    public class Tasks
    {
        public delegate void Action();

        public class TaskGenericAction : Task
        {
            Action mAction = null;

            Action[] mActions = null;

            public TaskGenericAction(Action PerformAction)
            {
                mAction = PerformAction;
            }   

            public TaskGenericAction(Action[] PerformActions)
            {
                mActions = PerformActions;
            }

            public override void Simulate()
            {
                try
                {
                    if (mAction != null)
                    {
                        mAction();
                    }
                    if (mActions != null && mActions.Length > 0)
                    {
                        foreach (Action action in mActions)
                        {
                            action();
                        }
                    }
                }
                catch
                {
                }
                finally 
                {
                    Sims3.SimIFace.Simulator.DestroyObject(base.ObjectId);
                }
            }

            public static void Start(Action PerformAction)
            {
                Simulator.AddObject(new TaskGenericAction(PerformAction));
            }

            public static void StartArray(Action[] PerformActions)
            {
                Simulator.AddObject(new TaskGenericAction(PerformActions));
            }
        }
    }
}
