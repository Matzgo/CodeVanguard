namespace Game
{
    public static class Safe
    {
        public static bool Locked = true;
        public static void Open()
        {

            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Opening Safe");
            CodeInspector.RuntimeManager.Instance.Trigger("SafeOpen");
        }

        public static void Alarm()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Playing Safe Alarm");
            CodeInspector.RuntimeManager.Instance.Trigger("SafeAlarm");

        }

    }
}