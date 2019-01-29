using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class JobQueue
{
	Queue<ConstructionJob> constructionJobs;
    Queue<HarvestJob> harvestJobs;
    Queue<PlantJob> plantJobs;

    List<ConstructionJob> deletedConstructionJobs;
    List<HarvestJob> deletedHarvestJobs;
    List<PlantJob> deletedPlantJobs;

    World world;

	public JobQueue (World world)
	{
		constructionJobs = new Queue<ConstructionJob> ();
        harvestJobs = new Queue<HarvestJob>();
        plantJobs = new Queue<PlantJob>();

        deletedConstructionJobs = new List<ConstructionJob>();
        deletedHarvestJobs = new List<HarvestJob>();
        deletedPlantJobs = new List<PlantJob>();
        this.world = world;
	}

    /**
     * Construction jobs
     */ 
    public void OfferConstructionJob(ConstructionJob job)
    {
        Debug.Log("Offering construction job for entity" + job.Addition.GetHashCode());
        if (constructionJobs.Contains(job))
        {
            Debug.LogError("Trying to add a job to the queue that already exists");
            return;
        }

        job.OnJobDelete += RemoveJob;

        constructionJobs.Enqueue(job);
    }

    public ConstructionJob RequestConstructionJob()
    {
        if (constructionJobs.Count > 0)
        {
            ConstructionJob cj = constructionJobs.Dequeue();
            if (deletedConstructionJobs.Contains(cj))
            {
                return RequestConstructionJob();
            } else
            {
                return cj;
            }
        }
            
        return null;
    }

    /**
     * Harvest jobs
     */
    public void OfferHarvestJob(HarvestJob job)
    {
        Debug.Log("Offering harvest job");
        if (harvestJobs.Contains(job))
        {
            Debug.LogError("Trying to add a job to the harvest queue that already exists");
            return;
        }
        job.OnJobDelete += RemoveJob;
        harvestJobs.Enqueue(job);
    }

    public HarvestJob RequestHarvestJob()
    {
        if (harvestJobs.Count > 0)
        {
            HarvestJob hj = harvestJobs.Dequeue();
            if (harvestJobs.Contains(hj))
            {
                return RequestHarvestJob();
            }
            else
            {
                return hj;
            }
        }
        return null;
    }

    /**
    * Harvest jobs
    */
    public void OfferPlantJob(PlantJob job)
    {
        Debug.Log("Offering plant job");
        if (plantJobs.Contains(job))
        {
            Debug.LogError("Trying to add a job to the plant queue that already exists");
            return;
        }
        job.OnJobDelete += RemoveJob;
        plantJobs.Enqueue(job);
    }

    public PlantJob RequestPlantJob()
    {
        Debug.Log("Requesting plant job");
        if (plantJobs.Count > 0)
        {
            PlantJob pj = plantJobs.Dequeue();
            if (deletedPlantJobs.Contains(pj))
            {
                return RequestPlantJob();
            }
            else
            {
                return pj;
            }
        }
        return null;
    }

    /// <summary>
    /// Don't like this, it's ugly, but I'm too lazy to figure out a proper solution for now
    /// It won't get called often, just once per character with a job every time you save so the performance loss shouldn't be the end of the world
    /// </summary>
    /// <param name="job"></param>
    public void RequeueJob(Job job)
    {
        if(job is ConstructionJob)
        {
            OfferConstructionJob((ConstructionJob)job);
            return;
        }
        else if (job is HarvestJob)
        {
            OfferHarvestJob((HarvestJob)job);
            return;
        }
        else if (job is PlantJob)
        {
            OfferPlantJob((PlantJob)job);
            return;
        }

        if (job is MovementJob)
            return;

        Debug.LogError("Offering a job to the queue that we cannot put in any specific queue?");
    }

    protected void RemoveJob(Job job)
    {
        if (job is ConstructionJob)
        {
            deletedConstructionJobs.Add((ConstructionJob)job);
        }
        else if (job is HarvestJob)
        {
            deletedHarvestJobs.Add((HarvestJob)job);
        }
        else if (job is PlantJob)
        {
            deletedPlantJobs.Add((PlantJob)job);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 										SERIALIZATION
    /// 
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("JobQueue");

        writer.WriteStartElement("ConstructionJobs");
        foreach(ConstructionJob job in constructionJobs)
        {
            job.WriteXml(writer);
        }
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader, World world)
    {
        // The order of reading is important, construction jobs always come first
        if (reader.ReadToDescendant("ConstructionJobs"))
        {
            Debug.Log("Reading Construction Jobs");
            int count = 0;
            if (reader.ReadToDescendant("Job"))
            {
                // We have at least one job to read
                do
                { // Read it while there are more jobs to read
                    ConstructionJob job = new ConstructionJob(null);
                    job.ReadXml(reader, world);
                    constructionJobs.Enqueue(job);
                    count++;
                } while (reader.ReadToNextSibling("Job"));
            }
            Debug.Log("Read " + count + " Construction jobs");
        }
    }

    protected virtual void WriteAdditionalXmlProperties(XmlWriter writer)
    {

    }

    protected virtual void ReadAdditionalXmlProperties(XmlReader reader)
    {

    }
}
