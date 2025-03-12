using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class CraneController
{
	public Crane crane;

	public void MoveItems()
	{
			crane.PickUp();
		wwwwwddddddd	crane.MoveRight();
			crane.MoveRight();
			crane.MoveLeft();
			crane.Drop();
			crane.MoveLeft();

		
	}
}