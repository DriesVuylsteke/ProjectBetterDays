using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class JobQueue
{
	Queue<ConstructionJob> constructionJobs;
    Queue<HarvestJob> harvestJobs;
    Queue<PlantJob> plantJobs;
    Queue<HaulJob> haulingJobs;

    List<ConstructionJob> deletedConstructionJobs;
    List<HarvestJob> deletedHarvestJobs;
    List<PlantJob> deletedPlantJobs;
    List<HaulJob> deletedHaulingJobs;

    World world;

	public JobQueue (World world)
	{
		constructionJobs = new Queue<ConstructionJob> ();
        harvestJobs = new Queue<HarvestJob>();
        plantJobs = new Queue<PlantJob>();
        haulingJobs = new Queue<HaulJob>();

        deletedConstructionJobs = new List<ConstructionJob>();
        deletedHarvestJobs = new List<HarvestJob>();
        deletedPlantJobs = new List<PlantJob>();
        deletedHaulingJobs = new List<HaulJob>();
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

        job.OnJobDelete += MarkJobDepricated;

        constructionJobs.Enqueue(job);
    }

    public ConstructionJob RequestConstructionJob()
    {
        if (constructionJobs.Count > 0)
        {
            ConstructionJob cj = constructionJobs.Dequeue();
            if (deletedConstructionJobs.Contains(cj))
            {
                deletedConstructionJobs.Remove(cj);
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
        job.OnJobDelete += MarkJobDepricated;
        harvestJobs.Enqueue(job);
    }

    public HarvestJob RequestHarvestJob()
    {
        if (harvestJobs.Count > 0)
        {
            HarvestJob hj = harvestJobs.Dequeue();
            if (deletedHarvestJobs.Contains(hj))
            {
                deletedHarvestJobs.Remove(hj);
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
        job.OnJobDelete += MarkJobDepricated;
        plantJobs.Enqueue(job);
    }

    public PlantJob RequestPlantJob()
    {
        if (plantJobs.Count > 0)
        {
            PlantJob pj = plantJobs.Dequeue();
            if (deletedPlantJobs.Contains(pj))
            {
                deletedPlantJobs.Remove(pj);
                return RequestPlantJob();
            }
            else
            {
                return pj;
            }
        }
        return null;
    }

    /**
    * Haul jobs
    */
    public void OfferHaulJob(HaulJob job)
    {
        Debug.Log("Offering Haul job");
        if (haulingJobs.Contains(job))
        {
            Debug.LogError("Trying to add a job to the Haul queue that already exists");
            return;
        }
        job.OnJobDelete += MarkJobDepricated;
        haulingJobs.Enqueue(job);
    }

    public HaulJob RequestHaulJob()
    {
        if (haulingJobs.Count > 0)
        {
            HaulJob hj = haulingJobs.Dequeue();
            if (deletedHaulingJobs.Contains(hj))
            {
                deletedHaulingJobs.Remove(hj);
                return RequestHaulJob();
            }
            else
            {
                return hj;
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
        else if (job is HaulJob)
        {
            OfferHaulJob((HaulJob)job);
            return;
        }

        if (job is MovementJob)
            return;

        Debug.LogError("Offering a job to the queue that we cannot put in any specific queue?");
    }

    protected void MarkJobDepricated(Job job)
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
        else if (job is HaulJob)
        {
            deletedHaulingJobs.Add((HaulJob)job);
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
        foreach (ConstructionJob job in constructionJobs)
        {
            job.WriteXml(writer);
        }
        writer.WriteEndElement();

        writer.WriteStartElement("HarvestJobs");
        foreach (HarvestJob job in harvestJobs)
        {
            job.WriteXml(writer);
        }
        writer.WriteEndElement();

        writer.WriteStartElement("PlantJobs");
        foreach (PlantJob job in plantJobs)
        {
            job.WriteXml(writer);
        }
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader, World world)
    {
        reader.ReadToDescendant("ConstructionJobs");
        ReadJobList("ConstructionJobs", constructionJobs, reader, world, new ConstructionJob(null));
        reader.ReadToNextSibling("HarvestJobs");
        Debug.Log(reader.Name);
        ReadJobList("HarvestJobs", harvestJobs, reader, world, new HarvestJob(null));
        reader.ReadToNextSibling("PlantJobs");
        Debug.Log(reader.Name);
        ReadJobList("PlantJobs", plantJobs, reader, world, new PlantJob(null));
    }

    private void ReadJobList<T>(string name, Queue<T> jobList, XmlReader reader, World world, T instanceToClone) where T : Job
    {
        // Read the construction jobs if there are any
        Debug.Log("Reading " + name + " Jobs");
        int count = 0;
        if (reader.ReadToDescendant("Job"))
        {
            // We have at least one job to read
            do
            { // Read it while there are more jobs to read
                T job = (T)instanceToClone.Clone();
                job.ReadXml(reader, world);
                jobList.Enqueue(job);
                count++;
            } while (reader.ReadToNextSibling("Job"));
        }
        Debug.Log("Read " + count + " " + name + " jobs");
    }

    protected virtual void WriteAdditionalXmlProperties(XmlWriter writer)
    {

    }

    protected virtual void ReadAdditionalXmlProperties(XmlReader reader)
    {

    }
}
