using System.Xml;

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
}
