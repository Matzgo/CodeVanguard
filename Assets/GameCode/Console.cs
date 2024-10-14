namespace Game
{
    public static class Console
    {

        public static void Write(string message)
        {

            CodeInspector.RuntimeManager.Instance.Console.Write(message);
        }

        public static void WriteLine(string message)
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine(message);
        }

        public static void Write(object a)
        {
            CodeInspector.RuntimeManager.Instance.Console.Write(a.ToString());
        }

        public static void WriteLine(object a)
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine(a.ToString());
        }

    }
}