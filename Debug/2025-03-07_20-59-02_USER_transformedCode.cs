using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class SafeSecurity
{
	public Safe safe;

	public void OpenSafeButton()
	{
		bool LOCKED = false;
		if(LOCKED)
		{
			safe.Alarm();
		}
		else
		{
			safe.Open();
		}
	}
}