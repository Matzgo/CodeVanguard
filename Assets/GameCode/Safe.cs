namespace Game
{
    public class Safe
    {
        public bool Locked = true;
        public void Open()
        {

            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Opening Safe");
            CodeInspector.RuntimeManager.Instance.Trigger("SafeOpen");
        }

        public void Alarm()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Playing Safe Alarm");
            CodeInspector.RuntimeManager.Instance.Trigger("SafeAlarm");

        }

    }
}