using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Plant : TileAddition
{
    protected int progressStates;
    protected float harvestAmount; // Don't save this on serialize, just start harvesting again
    protected float harvestSpeed = 0.2f;

    bool queuedForHarvest = false;

    public Plant(Tile tile) : base(tile)
    {
        SetupPlant();
    }

    public Plant() : base()
    {
        SetupPlant();
    }

    private void SetupPlant()
    {
        movementCost = 0.5f;
        constructionCost = 1;
        harvestAmount = 0f;
        //progressSpeed = 0.016f;
        //progressSpeed = 0.08f;
    }

    public override bool Conditions()
    {
        return this.tile.TileType == TileType.Floor &&
            this.tile.Addition != null &&
            this.tile.Addition is Soil &&
            this.tile.Addition.BuildPercentage == 1;
    }

    protected int GetProgressState()
    {
        return (int)(Progress / (1 / (float)progressStates));
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // The door hasn't been built yet! don't do anything
        if (BuildPercentage < 1)
            return;

        if (Progress >= 1) // Fully grown, later we could perhaps let the plants rot?
        {
            if (!queuedForHarvest)
            {
                queuedForHarvest = true;
                QueueHarvest();
            }
            return;
        }
        int curStage = GetProgressState();
        Progress += deltaTime * this.progressSpeed;
        if(curStage != GetProgressState())
        {
            DoWork(0);
        }
    }

    /// <summary>
    /// Queues a harvest job for this plant
    /// </summary>
    protected void QueueHarvest()
    {
        // For now just let the world know that this plant should be harvested
        HarvestJob harvestJob = new HarvestJob(this);
        harvestJob.OnJobDelete += (job) => { QueueHarvest(); };
        harvestJob.OnJobComplete += (job) =>
        {
            PlantHarvested(job);
        };
        tile.world.Jobs.EnqueueJob(harvestJob);
    }

    // Work in the case of a plant is harvesting it if it is fully grown
    // In case it is not work might be (later on) supplying nutrition (water) for example
    // It might be trimming the leaves, basicly things that need to be done in order for the plant to survive
    public override float DoWork(float amount)
    {
        base.DoWork(amount);
        // In case the plant is fully grown we don't want to construct, but harvest it instead
        if (Progress >= 1) {
            harvestAmount += amount * harvestSpeed;
        }
        return harvestAmount;
    }

    // Job is the harvest job that assigned this plant to be harvested
    protected abstract void PlantHarvested(Job job);
    protected abstract void HarvestCancelled(Job job);

    /// <summary>
    /// Resets the plant values, also offers a new job to plant this plant again
    /// </summary>
    protected virtual void ResetPlantValues()
    {
        BuildPercentage = 0;
        Progress = 0;
        SetupPlant();
        queuedForHarvest = false;

        // Notify everybody that values have changed
        // Do this by performing work (no actual work)
        DoWork(0);

        PlantJob job = new PlantJob(this);
        tile.world.Jobs.EnqueueJob(job);
    }

    public override bool CanContainItemOnTile(ItemStack stack)
    {
        // You can put an item on top of a plant
        return true;
    }
}
