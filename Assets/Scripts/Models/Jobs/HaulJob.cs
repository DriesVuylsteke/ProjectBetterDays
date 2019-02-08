using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class HaulJob : Job
{
    Tile dropoff;
    Character pawnThatPickedUpItem;

    public HaulJob(Tile pickup, Tile dropoff) : base()
    {
        DestinationTile = pickup;
        this.dropoff = dropoff;
        StandOnDestination = true;
    }

    /// <summary>
    /// ONLY USE THIS FOR DESERIALIZATION, THIS JOB WILL NOT HAVE SUFFICIENT DATA TO WORK WITHOUT READING ADDITIONAL INFORMATION FROM THE XML FILE
    /// </summary>
    public HaulJob() : base() { }

    public override void DoWork(Character pawnDoingJob, float deltaTime)
    {
        // Pick up the stack once
        if (pawnThatPickedUpItem == null)
        {
            pawnDoingJob.HeldItem = DestinationTile.TakeItemStackFromTile(pawnDoingJob.HeldItem);
            pawnThatPickedUpItem = pawnDoingJob;
            DestinationTile = dropoff;
        } else
        {
            // We should have the item in our inventory so drop it, no work will be done if we aren't on top of the tile
            // Not sure what to do if the item can't be dropped off. I'll figure this out later?
            pawnDoingJob.HeldItem = DestinationTile.AddItemStackToTile(pawnDoingJob.HeldItem);
            JobComplete();
        }
    }

    protected override void OnJobCancelled(Job job)
    {
        if (pawnThatPickedUpItem != null)
        {
            // Cancelling this job should delete this job instead of cancelling it
            // We'll create the new job on our own because the pickup tile will have changed
            HaulJob updated = new HaulJob(pawnThatPickedUpItem.CurrTile, dropoff);
            pawnThatPickedUpItem.HeldItem = pawnThatPickedUpItem.CurrTile.AddItemStackToTile(pawnThatPickedUpItem.HeldItem);
            DestinationTile.world.Jobs.EnqueueJob(updated);
            job.DeleteJob();
        } else
        {
            // We didn't reach the destination tile so just cancel it like always
            base.OnJobCancelled(job);
        }
    }

    public override Skills GetJobType()
    {
        return Skills.Speed;
    }

    public override void EnqueueFromSubclass(JobQueue theQueue, bool firstItem = false)
    {
        if (firstItem)
        {
            theQueue.EnqueueJobAndResetQueue(this);
        }
        else
        {
            theQueue.EnqueueJob(this);
        }
    }
}