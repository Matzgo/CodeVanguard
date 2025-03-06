using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class GeneratorController
{
	
	
	int beamCount;
	public void HandleBeam()
	{
		beamCount++;	

		Generator.RedirectBeam();
		
	}

	public void TestBeams()
	{
		Generator.FireBeam();
		HandleBeam();
		Generator.FireBeam();
		HandleBeam();
		Generator.FireBeam();
		HandleBeam();
		Generator.FireBeam();
		HandleBeam();
		Generator.FireBeam();
		HandleBeam();
		Generator.FireBeam();
		HandleBeam();
		Generator.Start();
	}

	

}