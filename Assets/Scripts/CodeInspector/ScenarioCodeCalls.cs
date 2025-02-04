using RoslynCSharp;

public static class ScenarioCodeCalls
{
    public static void DoTestCall(ScenarioType type, ScriptProxy userScript)
    {
        switch (type)
        {
            case ScenarioType.Crane:
                userScript.Call("MoveItems");
                break;
            case ScenarioType.Safe:
                userScript.Call("OpenSafeButton");
                break;
            case ScenarioType.Power:
                userScript.Call("TestBeams");
                //userScript.Call("TestBeams");
                break;
        }
    }
}
