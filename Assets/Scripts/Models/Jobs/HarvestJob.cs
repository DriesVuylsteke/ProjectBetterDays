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


    protected void TileAdditionRemoved(TileAddition addition)
    {
        DeleteJob();
    }

    public override void DoWork(float amount)
    {
        if (Addition.DoWork(amount) >= 1)
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

    public override Job Clone()
    {
        return new HarvestJob(Addition);
    }
}

