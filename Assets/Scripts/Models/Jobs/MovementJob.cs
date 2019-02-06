﻿using System;
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

    public override Job Clone()
    {
        return new MovementJob(DestinationTile);
    }

    public override void DoWork(float amount)
    {
        JobComplete();
    }

    public override Skills GetJobType()
    {
        return Skills.Speed;
    }
}
