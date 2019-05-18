using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;

public class JobQueue
{
    World world;

    // Really ugly, all of the jobs here are duplicates from those in their queues, but I can't figure out how to access all of the PerType classes at runtime
    Dictionary<string, SpecificQueue> activeQueuesWithKey; // made based on activeQueues every tim 

    // the string is the newly added queue identifier
    public event Action<JobQueue, string> JobQueueAdded;

    public JobQueue(World world)
    {
        this.world = world;
        activeQueuesWithKey = new Dictionary<string, SpecificQueue>();
    }

    private abstract class SpecificQueue
    {
        public abstract void WriteXml(XmlWriter writer);
        public abstract string GetDescription();
        public abstract Job RequestJob();
    }

    private class SpecificQueue<T> : SpecificQueue where T : Job
    {
        private static SpecificQueue<T> instance;
        public static SpecificQueue<T> Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new SpecificQueue<T>();
                    string classDescription = instance.GetType().ToString();
                    int indexOpen = classDescription.IndexOf('[');
                    int indexClose = classDescription.IndexOf("Job]");
                    instance.Title = classDescription.Substring(indexOpen + 1, (indexClose - indexOpen - 1));
                }
                return instance;
            }
            private set{ instance = value; }
        }

        private SpecificQueue(){} // singleton
        private Queue<T> queue = new Queue<T>();
        private List<T> depricatedJobs = new List<T>();
        protected string Title { set; get; }

        /// <summary>
        /// Hard resets the queue, this should only be used when deserialisation is done and a new "instance" is started. This is to bypass that old data is used (since the queues are static)
        /// </summary>
        public void HardResetQueue() { instance = null; }

        public void OfferJob(T job)
        {
            if (queue.Contains(job))
            {
                UnityEngine.Debug.LogError("Trying to add a job to the queue that already exists");
                return;
            }

            job.OnJobDelete += (j) =>
            {
                if(j is T)
                {
                    depricatedJobs.Add((T)j);
                } else
                {
                    UnityEngine.Debug.LogError("A job is being deleted and telling the wrong queue about it");
                }
            };

            queue.Enqueue(job);
        }

        // If the need ever arrives to request a specific job, we can always make a method RequestJobOfSpecifiedType

        public override Job RequestJob()
        {
            if (queue.Count > 0)
            {
                T topJob = queue.Dequeue();
                // The job got somehow added back to the queue while no longer being valid (as in it was deleted)
                // This is a memory leak that should clean itself up here I guess.
                // PERFORMANCE: Log how often this happens and if it's too often perhaps prevent it.
                if (depricatedJobs.Contains(topJob))
                {
                    depricatedJobs.Remove(topJob);
                    return RequestJob();
                }
                else
                {
                    return topJob;
                }
            }

            return null;
        }

        public override void WriteXml(XmlWriter writer)
        {
            foreach(T job in queue)
            {
                job.WriteXml(writer);

                // Could be tempted to remove the job here. But saving doesn't mean quitting so we still need all this information.
                // The issue here is now that when loading we do want to reset all queues so we'll have to do that manually
            }
        }

        public override string GetDescription()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Returnes a list of all the names for all active queues
    /// </summary>
    /// <returns></returns>
    public List<string> GetActiveQueues()
    {
        return activeQueuesWithKey.Keys.ToList();
    }

    public Job RequestJob(List<String> priorities)
    {
        foreach(string priority in priorities)
        {
            if (activeQueuesWithKey.ContainsKey(priority))
            {
                SpecificQueue queue = activeQueuesWithKey[priority];
                Job requestedJob = queue.RequestJob();
                if(requestedJob != null)
                {
                    return requestedJob;
                }
                // No job? check for a lower priority job!
            } else
            {
                UnityEngine.Debug.Log("JobQueue -- A priority string was passed that we do not have a queue for, you probably edited the list instead of just reorganised it");
                return null;
            }
        }
        // Do not remove empty queues, this allows us to request all jobtypes that have ever been active in the world.
        // This is useful for setting up priorities

        // None of the queues had a job, the pawn is now idle
        return null;
    }

    /// <summary>
    /// Enqueue a job in it's designated queue
    /// </summary>
    /// <typeparam name="T">The type of the job you are enqueueing</typeparam>
    /// <param name="job">The job to enqueue</param>
    public void EnqueueJob<T>(T job) where T : Job
    {
        SpecificQueue<T> queue = SpecificQueue<T>.Instance;
        UnityEngine.Debug.Log("Enqueuing job of type " + job.GetType() + " for queue: " + queue.GetType());
        if (activeQueuesWithKey.Keys.Contains(queue.GetDescription()))
        {
            // We are keeping track of the queue
        } else
        {
            // This is a job type that we don't have a queue for yet! Add it to the list of tracked job queues so the user can see it exists
            activeQueuesWithKey.Add(queue.GetDescription(), queue);
            if(JobQueueAdded != null)
            {
                JobQueueAdded(this, queue.GetDescription());
            }
        }

        // The reason to do this here is because here we know the job is properly instantiated but hasn't been enqueued yet.
        // If a job is directly assigned to a character we don't need it to pop up on the tile.

        // Makes sure that the active tile is aware of it's currently active job. Unregistering is done by the tile itself.
        job.GetActiveTile().AddJob(job);
        // If the active tile is updated, the new tile should be informed
        job.OnJobDestinationUpdated += (j) => { j.GetActiveTile().AddJob(j); };
        queue.OfferJob(job);
    }

    /// <summary>
    /// Enqueue a job in it's designated queue, but reset the queue before enqueueing it. Only use when adding a job for the first time after deserialisation
    /// </summary>
    /// <typeparam name="T">The type of the job you are enqueueing</typeparam>
    /// <param name="job">The job to enqueue</param>
    public void EnqueueJobAndResetQueue<T>(T job) where T : Job
    {
        
        SpecificQueue<T> queue = SpecificQueue<T>.Instance;
        UnityEngine.Debug.Log("Enqueuing job of type " + job.GetType() + " and resetting queue: " + queue.GetType());
        queue.HardResetQueue();
        EnqueueJob(job);
    }

    private T CastToCorrectJob<T>(Job job) where T : Job
    {
        return (T)job;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 										SERIALIZATION
    /// 
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("JobQueue");

        foreach(SpecificQueue t in activeQueuesWithKey.Values)
        {
            t.WriteXml(writer);
        }

        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader, World world)
    {
        //reader.ReadToDescendant("ConstructionJobs");
        int count = 0;
        // ONLY USE THIS FOR SINGLE THREADED LOADING!!!!!!!
        Assembly assembly = new StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
        HashSet<string> jobTypesRead = new HashSet<string>();
        if (reader.ReadToDescendant("Job"))
        {
            // We have at least one job to read
            do
            { // Read it while there are more jobs to read
                string className = reader.GetAttribute("Class");
                System.Runtime.Remoting.ObjectHandle oh = Activator.CreateInstanceFrom(assembly.CodeBase, className);
                Job job = (Job) Convert.ChangeType(oh.Unwrap(), Type.GetType(className));
                
                if (jobTypesRead.Contains(className))
                    job.ReadXml(reader, world, this);
                else
                    job.ReadXml(reader, world, this, true);

                jobTypesRead.Add(className);
                count++;
            } while (reader.ReadToNextSibling("Job"));
        }
    }
}