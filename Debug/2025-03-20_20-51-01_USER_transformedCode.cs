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
			        for (int i = 0; i < 6; i++)
        {
            crane.PickUp();
            crane.MoveRight();
            crane.MoveRight();
            crane.MoveRight();
            crane.Drop();
            crane.MoveLeft();
            crane.MoveLeft();
            crane.MoveLeft();
        }

        // Move 4 items from Column 2 to Column 4
        crane.MoveRight(); // Move to Column 2
        for (int i = 0; i < 4; i++)
        {
            crane.PickUp();
            crane.MoveRight();
            crane.MoveRight();
            crane.Drop();
            crane.MoveLeft();
            crane.MoveLeft();
        }

		
	}
}