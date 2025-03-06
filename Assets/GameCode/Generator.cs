namespace Game
{
    public class Generator
    {
        public void FireBeam()
        {

            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Firing Beam");
            CodeInspector.RuntimeManager.Instance.Trigger("FireBeam");
        }

        public void RedirectBeam()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Redirecting Beam");
            CodeInspector.RuntimeManager.Instance.Trigger("RedirectBeam");

        }
        public void Start()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Blocking Beam");
            CodeInspector.RuntimeManager.Instance.Trigger("Start");

        }
    }
}