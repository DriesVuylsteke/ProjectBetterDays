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

    #region properties

    #region Coordinates
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
    #endregion Coordinates

    #region Skills
    protected Dictionary<Skills, float> stats;
    public float Speed
    {
        get
        {
            return stats[Skills.Speed];
        }
        protected set
        {
            stats[Skills.Speed] = value;
        }
    }

    public float Construction
    {
        get
        {
            return stats[Skills.Construction];
        }
        protected set
        {
            stats[Skills.Construction] = value;
        }
    }

    public float Planting
    {
        get
        {
            return stats[Skills.Planting];
        }
        protected set
        {
            stats[Skills.Planting] = value;
        }
    }

    public float Harvesting
    {
        get
        {
            return stats[Skills.Harvesting];
        }
        protected set
        {
            stats[Skills.Harvesting] = value;
        }
    }
    #endregion

    public float ProgressToNextTile { get; set; }

	public bool Selected { get; protected set; }
    public World world;

    #region Inventory
    private ItemStack heldItemStack;

    public ItemStack HeldItem
    {
        get { return heldItemStack; }
        set { heldItemStack = value; }
    }

    #endregion Inventory

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
    public event Action<Character, Job> OnCharacterJobChanged; // Called when the character has a new job
    #endregion

    #region Job
    protected HashSet<String> allJobs;
    public List<string> jobPriorities; // TODO: make a getter for this that makes a deep copy and returns that, this seems unsafe to pass the actual list

    protected Job currentJob;
	protected Job CurrentJob {
		get {
			return currentJob;
		}
        set
        {
            if(currentJob != null && currentJob != value)
            {
                currentJob.OnJobComplete -= OnJobComplete;
                currentJob.OnJobDelete -= OnJobDelete;
                currentJob.OnJobDestinationUpdated -= OnJobDestinationChanged;
            }
            jobReached = false;
            currentJob = value;
            if (currentJob != null)
            {
                currentJob.OnJobComplete += OnJobComplete;
                currentJob.OnJobDelete += OnJobDelete;
                currentJob.OnJobDestinationUpdated += OnJobDestinationChanged;
            }
            if(OnCharacterJobChanged != null)
            {
                OnCharacterJobChanged(this, currentJob);
            }
        }
	}
	protected bool jobReached = false;
    #endregion

    protected AStar pathfinding;

    #endregion properties

    public Character(Tile tile, World world, float speed = 1, float construction = 1, float planting = 1, float harvesting = 1)
    {
        stats = new Dictionary<Skills, float>();
        stats.Add(Skills.Speed, speed);
        stats.Add(Skills.Construction, construction);
        stats.Add(Skills.Planting, planting);
        stats.Add(Skills.Harvesting, harvesting);

        this.Speed = speed;
        this.world = world;

		CurrTile = tile;
		NextTile = tile;
		DestTile = tile;

        // Register to update your list of possible jobs
        world.Jobs.JobQueueAdded += Jobs_JobQueueAdded;
        allJobs = new HashSet<string>();
        jobPriorities = new List<string>();
        foreach(string jobName in world.Jobs.GetActiveQueues())
        {
            allJobs.Add(jobName);
            if (!jobPriorities.Contains(jobName))
            {
                jobPriorities.Add(jobName);
            }
        }
	}

    // TODO: should probably not give a hard copy here
    /// <summary>
    /// Gives the player stats
    /// </summary>
    /// <returns>The player stats</returns>
    public Dictionary<Skills, float> GetPlayerStats() { return stats; }

    #region characterSelection

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

    #endregion characterSelection

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
                    Debug.Log("Deleting job");
                    CurrentJob.DeleteJob();
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
                // This is also where we gain experience for the work done
                Skills jobSkill = CurrentJob.GetJobType();
                float skillLvl = stats[jobSkill];
                CurrentJob.DoWork(this, deltaTime);
                // Xp gained is based on time spent working, so more work (due to higher level) doesn't equal more xp
                float xpAmount = deltaTime / (skillLvl * 60);
                stats[jobSkill] += xpAmount;
			}

			// If we don't have a job look for one
			if (CurrentJob == null) {
                // Request a job from the world
                // TODO: subclass this so different characters can request different jobs
                // TODO: this gets spammed if there are no jobs, perhaps make the character idle for a couple seconds? Could also be a job!
                // TODO: move this request into the job queue, should probably pass your own preferences of jobs in that function too
                Job j = world.Jobs.RequestJob(jobPriorities);
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
            // TODO: this is ugly?
            CurrentJob.EnqueueFromSubclass(world.Jobs);
        }

        CurrentJob = job;
        DestTile = currentJob.DestinationTile;
        NextTile = CurrTile;
    }

    private void Jobs_JobQueueAdded(JobQueue theQueue, string queueDescription)
    {
        if (allJobs.Contains(queueDescription))
        {
            Debug.LogError("The jobqueue tells us about a new job that we already know about?");
        } else
        {
            allJobs.Add(queueDescription);
            jobPriorities.Add(queueDescription); // Add the new job as the lowest priority
            Debug.Log("Added job priority: " + queueDescription);
            // Also add the job to the priority list with the lowest priority
        }
    }

    /// <summary>
    /// Assigns a priority to a certain type of jobQueue
    /// </summary>
    /// <param name="jobIdentifier">An identifier present in allJobs</param>
    /// <param name="priority">A priority for the job, should be a value between 0 and allJobs.Size with 0 being the highest priority </param>
    public void SetJobPriority(string jobIdentifier, int priority)
    {
        if(priority < 0 || priority >= jobPriorities.Count)
        {
            Debug.LogError("Not a valid priority, max priority: " + (jobPriorities.Count-1) + " Given priority: " + priority);
            return;
        }
        if (!jobPriorities.Contains(jobIdentifier))
        {
            Debug.LogError("Not a valid jobIdentifier: " + jobIdentifier);
            return;
        }

        if(jobPriorities.IndexOf(jobIdentifier) == priority)
        {
            // Already in the right spot, just return
            return;
        }

        Debug.Log("Changing a job priority: " + jobIdentifier + " to " + priority);

        jobPriorities.Remove(jobIdentifier);
        List<string> reSortedJobs = new List<string>();

        for(int i = 0; i < jobPriorities.Count; i++)
        {
            if(i == priority)
            {
                reSortedJobs.Add(jobIdentifier);
            }
            reSortedJobs.Add(jobPriorities[i]);
        }
        // only occurs when we move to the end of the queue
        if(priority == reSortedJobs.Count)
        {
            reSortedJobs.Add(jobIdentifier);
        }

        jobPriorities = reSortedJobs;
        Debug.Log("Changing a job priority: " + jobPriorities.Count);
    }

    #region JobEvents
    void OnJobDestinationChanged(Job job)
    {
        DestTile = job.DestinationTile;
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

    #endregion

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 										SERIALIZATION
    /// 
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Serialization
    public void WriteXml (XmlWriter writer) {

		writer.WriteStartElement ("Character");

        foreach (Skills skill in (Skills[])Enum.GetValues(typeof(Skills)))
        {
            writer.WriteAttributeString("Skill" + (int)skill, stats[skill].ToString());
        }

        writer.WriteAttributeString("Priorities", jobPriorities.Count.ToString());
        int i = 0;
        foreach(string priority in jobPriorities)
        {
            writer.WriteAttributeString("Priority" + i, priority);
            i++;
        }

        // currtile -- x and y position
        writer.WriteAttributeString ("X", currTile.X.ToString());
		writer.WriteAttributeString ("Y", currTile.Y.ToString());

        if (CurrentJob != null)
        {
            CurrentJob.EnqueueFromSubclass(world.Jobs);
        }


		writer.WriteEndElement ();
	}

    /// <summary>
    /// Reads the caracter specific properties
    /// </summary>
	public void ReadXml(XmlReader reader){
        foreach (Skills skill in (Skills[])Enum.GetValues(typeof(Skills)))
        {
            stats[skill] = float.Parse(reader.GetAttribute("Skill"+(int)skill));
        }

        // First clear the priorities created from the existing jobs, no "new" jobs will have appeared since the game was saved
        jobPriorities.Clear();
        int prioritiesToRead = int.Parse(reader.GetAttribute("Priorities"));
        for(int i = 0; i < prioritiesToRead; i++)
        {
            jobPriorities.Add(reader.GetAttribute("Priority" + i));
        }

    }
    #endregion
}