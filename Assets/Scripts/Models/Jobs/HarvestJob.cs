using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class HarvestJob : Job
{
    public TileAddition Addition { get; set; }

    public HarvestJob(TileAddition addition) : base()
    {
        Addition = addition;

        if (addition != null)
        {
            addition.TileAdditionRemoved += TileAdditionRemoved;
            DestinationTile = addition.tile;
        }
    }

    /// <summary>
    /// ONLY USE THIS FOR DESERIALIZATION, THIS JOB WILL NOT HAVE SUFFICIENT DATA TO WORK WITHOUT READING ADDITIONAL INFORMATION FROM THE XML FILE
    /// </summary>
    public HarvestJob() : base() { }

    protected void TileAdditionRemoved(TileAddition addition)
    {
        DeleteJob();
    }

    public override void DoWork(Character pawnDoingJob, float deltaTime)
    {
        if (Addition.DoWork(CalculateWorkAmount(pawnDoingJob, deltaTime)) >= 1)
        {
            JobComplete();
        }
    }

    protected override void OnJobCancelled(Job job)
    {
        base.OnJobCancelled(job);
    }

    protected override void WriteAdditionalXmlProperties(XmlWriter writer)
    {
        base.WriteAdditionalXmlProperties(writer);
    }

    protected override void ReadAdditionalXmlProperties(XmlReader reader)
    {
        base.ReadAdditionalXmlProperties(reader);

        Addition = this.DestinationTile.Addition;
    }

    public override Skills GetJobType()
    {
        return Skills.Harvesting;
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

