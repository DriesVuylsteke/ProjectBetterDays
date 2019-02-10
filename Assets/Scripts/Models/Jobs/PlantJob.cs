using System.Xml;
using UnityEngine;

public class PlantJob : Job
{
    public TileAddition Addition { get; set; }

    public PlantJob(TileAddition addition) : base()
    {
        Addition = addition;
        
        if (addition != null)
        {
            addition.TileAdditionRemoved += TileAdditionRemoved;
            addition.TileAdditionBuilt += Addition_TileAdditionBuilt;
            DestinationTile = addition.tile;
        }
    }

    /// <summary>
    /// ONLY USE THIS FOR DESERIALIZATION, THIS JOB WILL NOT HAVE SUFFICIENT DATA TO WORK WITHOUT READING ADDITIONAL INFORMATION FROM THE XML FILE
    /// </summary>
    public PlantJob() : base() { }

    private void Addition_TileAdditionBuilt(TileAddition obj)
    {
        JobComplete();
        Addition.TileAdditionBuilt -= Addition_TileAdditionBuilt;
    }

    protected void TileAdditionRemoved(TileAddition addition)
    {
        DeleteJob();
    }

    public override void DoWork(Character pawnDoingJob, float deltaTime)
    {
        Addition.DoWork(CalculateWorkAmount(pawnDoingJob, deltaTime));
    }

    protected override void OnJobCancelled(Job job)
    {
        base.OnJobCancelled(job);
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
        return Skills.Planting;
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
        return "Planting " + Addition.Name;
    }
}
