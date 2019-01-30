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
            DestinationTile = addition.tile;
        }

        addition.TileAdditionBuilt += Addition_TileAdditionBuilt;
    }

    private void Addition_TileAdditionBuilt(TileAddition obj)
    {
        JobComplete();
        Addition.TileAdditionBuilt -= Addition_TileAdditionBuilt;
    }

    protected void TileAdditionRemoved(TileAddition addition)
    {
        DeleteJob();
    }

    public override void DoWork(float amount)
    {
        Addition.DoWork(amount);
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

        Addition = this.DestinationTile.addition;
    }

    public override Skills GetJobType()
    {
        return Skills.Planting;
    }
}
