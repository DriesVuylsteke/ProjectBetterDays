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

    /// <summary>
    /// ONLY USE THIS FOR DESERIALIZATION, THIS JOB WILL NOT HAVE SUFFICIENT DATA TO WORK WITHOUT READING ADDITIONAL INFORMATION FROM THE XML FILE
    /// </summary>
    public MovementJob() : base() { }

    public override void DoWork(Character pawnDoingJob, float deltaTime)
    {
        JobComplete();
    }

    public override Skills GetJobType()
    {
        return Skills.Speed;
    }

    public override void EnqueueFromSubclass(JobQueue theQueue, bool firstItem = false)
    {
        // No need to enqueue movement jobs
    }
}
