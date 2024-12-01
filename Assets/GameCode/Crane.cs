namespace Game
{
    public static class Crane
    {

        public static void MoveLeft()
        {

            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Moving Left");
            CodeInspector.RuntimeManager.Instance.Trigger("MoveLeft");
        }

        public static void MoveRight()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Moving Right");
            CodeInspector.RuntimeManager.Instance.Trigger("MoveRight");

        }

        public static void PickUp()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Picking Up");
            CodeInspector.RuntimeManager.Instance.Trigger("PickUp");

        }

        public static void Drop()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Dropping");
            CodeInspector.RuntimeManager.Instance.Trigger("Drop");

        }

    }
}