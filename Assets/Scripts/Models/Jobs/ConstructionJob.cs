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

    protected void TileAdditionRemoved(TileAddition addition)
    {
        DeleteJob();
    }

	public override void DoWork (float amount){
		if (Addition.DoWork (amount) >= 1) {
			JobComplete ();
		}
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
        return Skills.Construction;
    }

    public override Job Clone()
    {
        return new ConstructionJob(Addition);
    }
}

