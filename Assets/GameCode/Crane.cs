namespace Game
{
    public class Crane
    {

        public void MoveLeft()
        {

            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Moving Left");
            CodeInspector.RuntimeManager.Instance.Trigger("MoveLeft");
        }

        public void MoveRight()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Moving Right");
            CodeInspector.RuntimeManager.Instance.Trigger("MoveRight");

        }

        public void PickUp()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Picking Up");
            CodeInspector.RuntimeManager.Instance.Trigger("PickUp");

        }

        public void Drop()
        {
            CodeInspector.RuntimeManager.Instance.Console.WriteLine("Dropping");
            CodeInspector.RuntimeManager.Instance.Trigger("Drop");

        }

    }
}