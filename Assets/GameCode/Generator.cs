namespace Game
{
    public static class Generator
    {
        public static void FireBeam()
        {

            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Firing Beam");
            CodeInspector.RuntimeManager.Instance.Trigger("FireBeam");
        }

        public static void RedirectBeam()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Redirecting Beam");
            CodeInspector.RuntimeManager.Instance.Trigger("RedirectBeam");

        }
        public static void Start()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Blocking Beam");
            CodeInspector.RuntimeManager.Instance.Trigger("Start");

        }
    }
}