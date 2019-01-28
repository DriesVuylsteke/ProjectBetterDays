using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;

public abstract class Job {

	public event Action<Job> OnJobComplete;
    public event Action<Job> OnJobCancel;
    public event Action<Job> OnJobDelete;

    public bool StandOnDestination { get; protected set; }

	// The tile that the current activity of the job takes place at
	public Tile DestinationTile { get; protected set;}

	public Job(){
		StandOnDestination = false;

        OnJobCancel += OnJobCancelled;
	}

	public abstract void DoWork (float amount);

	/// <summary>
	/// Called when the job is complete, probably from a subclass
	/// </summary>
	protected void JobComplete(){
		if (OnJobComplete != null) {
			OnJobComplete (this);
		}
        // Clean up the job after it is done (lets assume no job is repeatable, 
        // for repeatable jobs someone will queue it several times? or perhaps we addd a property later)
        OnJobComplete = null;
        OnJobCancel = null;
	}

    /// <summary>
    /// An outside source tells the job it should be cancelled
    /// For example, this could be because the job isn't reachable anymore.
    /// </summary>
    public void CancelJob()
    {
        if (OnJobCancel != null)
            OnJobCancel(this);
    }

    public void DeleteJob()
    {
        if(OnJobDelete != null)
        {
            OnJobDelete(this);
        }
    }

    protected virtual void OnJobCancelled(Job job) { }

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 
	/// 										SERIALIZATION
	/// 
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void WriteXml (XmlWriter writer) {
		writer.WriteStartElement ("Job");

        writer.WriteAttributeString("X", DestinationTile.X.ToString());
        writer.WriteAttributeString("Y", DestinationTile.Y.ToString());

        WriteAdditionalXmlProperties(writer);

        writer.WriteEndElement ();
	}

	public void ReadXml(XmlReader reader, World world){
        int X = int.Parse(reader.GetAttribute("X"));
        int Y = int.Parse(reader.GetAttribute("Y"));

        DestinationTile = world.GetTileAt(X,Y);

        ReadAdditionalXmlProperties(reader);
	}

	protected virtual void WriteAdditionalXmlProperties(XmlWriter writer){

	}

	protected virtual void ReadAdditionalXmlProperties(XmlReader reader){

	}
}
