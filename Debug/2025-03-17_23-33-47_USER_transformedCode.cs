using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class GeneratorController
{
	public Generator generator;
	
	int beamCount;
	public void HandleBeam()
	{
		beamCount++;	
		if(beamCount % 2 == 0)
		{
			generator.RedirectBeam();
		}
	}

	public void TestBeams()
	{
		generator.FireBeam();
		HandleBeam();
		generator.FireBeam();
		HandleBeam();
		generator.FireBeam();
		HandleBeam();
		generator.FireBeam();
		HandleBeam();
		generator.FireBeam();
		HandleBeam();
		generator.FireBeam();
		HandleBeam();
		generator.Start();
	}

	

}