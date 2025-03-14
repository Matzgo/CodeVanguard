using RoslynCSharp;

public static class ScenarioCodeCalls
{
    public static void DoTestCall(ScenarioType type, ScriptProxy userScript, Scenario scenario)
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
            case ScenarioType.Door:
                break;
            case ScenarioType.Antenna:
                userScript.Call("SortNumbers", (scenario as AntennaScenario).AwakeParams.startingArray);
                break;
        }
    }
}
