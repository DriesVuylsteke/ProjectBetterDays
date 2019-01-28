using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class MovementJob : Job
{
    public MovementJob(Tile tile)
    {
        DestinationTile = tile;
        StandOnDestination = true;
    }

    public override void DoWork(float amount)
    {
        JobComplete();
    }
}
