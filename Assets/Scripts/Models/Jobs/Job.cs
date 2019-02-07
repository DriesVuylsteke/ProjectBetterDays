﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;

public abstract class Job {

	public event Action<Job> OnJobComplete;
    public event Action<Job> OnJobCancel;
    public event Action<Job> OnJobDelete;
    public event Action<Job> OnJobDestinationUpdated;

    public bool StandOnDestination { get; protected set; }

    // The tile that the current activity of the job takes place at
    private Tile destinationTile;
	public Tile DestinationTile
    {
        get
        {
            return destinationTile;
        }
        set
        {
            if(value != destinationTile)
            {
                destinationTile = value;
                if (OnJobDestinationUpdated != null)
                {
                    OnJobDestinationUpdated(this);
                }
            }
        }
    }

	public Job(){
		StandOnDestination = false;

        OnJobCancel += OnJobCancelled;
	}

    /// <summary>
    /// Perform work on this job
    /// </summary>
    /// <param name="pawnDoingJob">The pawn performing the job</param>
    /// <param name="deltaTime">The amount of time worked on this job</param>
	public abstract void DoWork (Character pawnDoingJob, float deltaTime);

    /// <summary>
    /// Calculates the amount of work a character can do on this job
    /// </summary>
    /// <param name="character">The character doing the work</param>
    /// <param name="deltaTime">The amount of time worked on this job</param>
    /// <returns>The amount of work performed in one action</returns>
    protected float CalculateWorkAmount(Character character, float deltaTime)
    {
        // For now something very basic, might need to change this later
        return deltaTime * character.GetPlayerStats()[this.GetJobType()];
    }

    public abstract Job Clone();

	/// <summary>
	/// Called when the job is complete, probably from a subclass
	/// </summary>
	protected virtual void JobComplete(){
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
    public virtual void CancelJob()
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

    /// <summary>
    /// Gives the skill type of the job
    /// </summary>
    /// <returns></returns>
    public abstract Skills GetJobType();

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
