using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;

[SerializeField]
/// <summary>
/// The very basic character, has a position, speed and jobs
/// TODO: make characters that only accept certain jobs (example construction robot?)
/// TODO: make this abstract (template method hook for knowing which jobs to accept?")
/// </summary>
public class Character {

	//TODO: make this return a float based on the progress towards the destination tile
	public float X { 
		get{
			return CurrTile.X + (NextTile.X - CurrTile.X) * ProgressToNextTile;
		}
	}
	public float Y {
		get {
			return CurrTile.Y + (NextTile.Y - CurrTile.Y ) * ProgressToNextTile;
		}
	}


    protected Dictionary<string, float> stats;
    #region Statistics
    protected string speedKey = "Speed";
    public float Speed
    {
        get
        {
            return stats[speedKey];
        }
        protected set
        {
            stats[speedKey] = value;
        }
    }

    protected string constructionKey = "Construction";
    public float Construction
    {
        get
        {
            return stats[constructionKey];
        }
        protected set
        {
            stats[constructionKey] = value;
        }
    }

    protected string plantingKey = "Planting";
    public float Planting
    {
        get
        {
            return stats[plantingKey];
        }
        protected set
        {
            stats[plantingKey] = value;
        }
    }

    protected string harvestingKey = "Harvesting";
    public float Harvesting
    {
        get
        {
            return stats[harvestingKey];
        }
        protected set
        {
            stats[harvestingKey] = value;
        }
    }
    #endregion

    public float ProgressToNextTile { get; set; }

	public bool Selected { get; protected set; }
    public World world;

    #region Tiles
    protected Tile currTile;
	public Tile CurrTile { 
		get {
			return currTile;
		}
		set {
			currTile = value;
			if (CurrentJob != null && CurrentJob.StandOnDestination == false && jobReached == false) {
				float dist = Mathf.Sqrt (Mathf.Pow (CurrTile.X - DestTile.X, 2) + Mathf.Pow (CurrTile.Y - DestTile.Y, 2));
				if (dist < 2)
					jobReached = true;
			}
		}
	}
	protected Tile destTile;
	protected Tile DestTile { 
		get {
			return destTile;
		}
		set {
			destTile = value;

			if (CurrentJob != null && CurrentJob.StandOnDestination == false && jobReached == false) {
				float dist = Mathf.Sqrt (Mathf.Pow (CurrTile.X - DestTile.X, 2) + Mathf.Pow (CurrTile.Y - DestTile.Y, 2));
				if (dist < 2)
					jobReached = true;
			}

			pathfinding = null;
		}
	}
	public Tile NextTile { get; protected set; }
    #endregion

    #region Events
    public event Action<Character> OnCharacterPositionChanged;
	public event Action<Character> OnCharacterSelectedChanged;
    #endregion

    #region Job
    protected Job currentJob;
	protected Job CurrentJob {
		get {
			return currentJob;
		}
        set
        {
            if(currentJob != null && currentJob != value)
            {
                currentJob.OnJobCancel -= OnJobCancel;
                currentJob.OnJobComplete -= OnJobComplete;
                currentJob.OnJobDelete -= OnJobDelete;
            }
            jobReached = false;
            currentJob = value;
            if (currentJob != null)
            {
                currentJob.OnJobCancel += OnJobCancel;
                currentJob.OnJobComplete += OnJobComplete;
                currentJob.OnJobDelete += OnJobDelete;
            }
        }
	}
	protected bool jobReached = false;
    #endregion

    protected AStar pathfinding;

	public Character(Tile tile, World world, float speed = 1, float construction = 1, float planting = 1, float harvesting = 1)
    {
        stats = new Dictionary<string, float>();
        stats.Add(speedKey, speed);
        stats.Add(constructionKey, construction);
        stats.Add(plantingKey, planting);
        stats.Add(harvestingKey, harvesting);

        this.Speed = speed;
        this.world = world;

		CurrTile = tile;
		NextTile = tile;
		DestTile = tile;
	}

    // TODO: should probably not give a hard copy here
    /// <summary>
    /// Gives the player stats
    /// </summary>
    /// <returns>The player stats</returns>
    public Dictionary<string, float> GetPlayerStats() { return stats; }

	/// <summary>
	/// Attempts to select the character for actions
	/// </summary>
	/// <returns><c>true</c>, if character was selected, <c>false</c> otherwise.</returns>
	public bool SelectCharacter(){
		this.Selected = true;
		if (OnCharacterSelectedChanged != null)
			OnCharacterSelectedChanged (this);
		return true;
	}

	public void DeselectCharacter(){
		this.Selected = false;
		if (OnCharacterSelectedChanged != null)
			OnCharacterSelectedChanged (this);
	}

	public void Update(float deltaTime){
		// TODO: pathfinding

		// If we have a next tile, move to it
		if (jobReached == false && DestTile != CurrTile) { 
			// We have some place to be
			// Do we have pathfinding already?
			if (pathfinding == null) {
				// If not we should find new pathfinding.
				pathfinding = new AStar (DestTile.world.Graph, CurrTile, DestTile);
			}

			if (CurrTile == NextTile) {// We moved another step in the right direction 
				// If this is the first step on our journey it's fine because we just generated a path.
				// We might not just be moving inside of a room, but on the world scale
				NextTile = pathfinding.DequeueNextTile ();

                if(NextTile == null) // The pathfinding does not know where to go. Delete the current job, since only a job can make a character move
                {
                    Debug.Log("Cancelling job");
                    CurrentJob.CancelJob();
                    return;
                }
			}

			// MOVEMENT
			// We have a place to be, update our movement, lets assume that next tile is always 1 distance unit away
			// Can we move to the next tile?
			if (NextTile.IsEnterable ()) {
				ProgressToNextTile += deltaTime * Speed;
				if (ProgressToNextTile >= 1) {
					// We have reached the next tile!
					CurrTile = NextTile;
					ProgressToNextTile = 0;
				}
			}

			OnCharacterPositionChanged (this);
		} else {
			// We have reached our destination :)
			// do work on the current job
			if (CurrentJob != null) {
				CurrentJob.DoWork (deltaTime);
			}

			// If we don't have a job look for one
			if (CurrentJob == null) {
                // Request a job from the world
                // TODO: subclass this so different characters can request different jobs
                // TODO: this gets spammed if there are no jobs, perhaps make the character idle for a couple seconds? Could also be a job!
                Job j = world.Jobs.RequestConstructionJob();
                if(j == null)
                {
                    j = world.Jobs.RequestHarvestJob();
                }
                OverrideJob(j);

				if (CurrentJob == null) {
					// No job available for the moment, so lets just return and do nothing
					// TODO: make the character do something?
					return;
				}
			}
		}
	}

    public void OverrideJob(Job job)
    {
        if (job == null)
            return;

        if (CurrentJob != null)
        {
            Debug.Log("Requeueing job: " + CurrentJob.GetHashCode());
            world.Jobs.RequeueJob(CurrentJob);
        }

        CurrentJob = job;
        DestTile = currentJob.DestinationTile;
        NextTile = CurrTile;
    }

	void OnJobComplete(Job job){
		if (job == CurrentJob) {
            CurrentJob = null;
			DestTile = CurrTile;
			jobReached = false;
		} else {
			Debug.LogError ("A job that isn't our current job has told us it's complete, did you forget to unregister?");
		}
	}

    void OnJobCancel(Job job)
    {
        // For now it does the exact same thing as completing the job, but later we might want to differentiate so for now I'm leaving the duplicate code in.
        if (job == CurrentJob)
        {
            Debug.Log("Job cancelled");
            CurrentJob = null;
            DestTile = CurrTile;
            jobReached = false;
        }
        else
        {
            Debug.LogError("A job that isn't our current job has told us it's being cancelled, did you forget to unregister?");
        }
    }

    void OnJobDelete(Job job)
    {
        if (job == CurrentJob)
        {
            Debug.Log("Job deleted" + CurrentJob.GetHashCode());
            CurrentJob = null;
            DestTile = CurrTile;
            jobReached = false;
        }
        else
        {
            Debug.LogError("A job that isn't our current job has told us it's being cancelled, did you forget to unregister?");
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 										SERIALIZATION
    /// 
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Serialization
    public void WriteXml (XmlWriter writer) {

		writer.WriteStartElement ("Character");

		// speed
		writer.WriteAttributeString ("Speed", Speed.ToString());
		// currtile -- x and y position
		writer.WriteAttributeString ("X", currTile.X.ToString());
		writer.WriteAttributeString ("Y", currTile.Y.ToString());

        if (CurrentJob != null)
        {
            world.Jobs.RequeueJob(CurrentJob);
        }


		writer.WriteEndElement ();
	}

    /// <summary>
    /// Reads the caracter specific properties
    /// </summary>
	public void ReadXml(XmlReader reader){
        int speed = int.Parse(reader.GetAttribute("Speed"));
    }
    #endregion
}