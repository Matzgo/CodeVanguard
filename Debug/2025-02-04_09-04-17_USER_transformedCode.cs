using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class SafeSecurity
{
	public void OpenSafeButton()
	{
		bool LoCkeD = Safe.Locked;
		if(LoCkeD)
		{
			Safe.Alarm();
		}
		else
		{
			Safe.Open();
		}
	}
}