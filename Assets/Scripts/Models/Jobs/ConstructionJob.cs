using System;
using System.Xml;
using UnityEngine;

public class ConstructionJob : Job
{
	public TileAddition Addition { get; set;}

	public ConstructionJob (TileAddition addition) : base()
	{
		Addition = addition;
        
        if(addition != null)
        {
            DestinationTile = addition.tile;
            addition.TileAdditionRemoved += TileAdditionRemoved;
        }
	}

    /// <summary>
    /// ONLY USE THIS FOR DESERIALIZATION, THIS JOB WILL NOT HAVE SUFFICIENT DATA TO WORK WITHOUT READING ADDITIONAL INFORMATION FROM THE XML FILE
    /// </summary>
    public ConstructionJob() : base() { }

    protected void TileAdditionRemoved(TileAddition addition)
    {
        DeleteJob();
    }

	public override void DoWork (Character pawnDoingJob, float deltaTime)
    {
		if (Addition.DoWork (this.CalculateWorkAmount(pawnDoingJob, deltaTime)) >= 1) {
			JobComplete ();
		}
	}

    protected override void OnJobDeleted(Job job)
    {
        base.OnJobDeleted(job);
        Addition.tile.RemoveTileAddition();
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
        return Skills.Construction;
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

    public override string GetJobTitle()
    {
        return "Constructing " + Addition.Name;
    }
}

