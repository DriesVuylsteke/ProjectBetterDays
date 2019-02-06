using System;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public abstract class TileAddition
{
    // Called when a new texture should be loaded, the state has changed
	public event Action<TileAddition> WorkDone;
	public event Action<TileAddition> TileAdditionBuilt;
    public event Action<TileAddition> TileAdditionRemoved;

	public Tile tile { get; protected set;}
	/// <summary>
	/// The name identifying the type of tile addition
	/// </summary>
	public string Name; // Set in the subclasses constructor
	protected float orientation; // The angle the texture should be rendered at (don't think this really belongs here, but I have no clue where else to put it)
	protected string renderString;
	// Could argue that it belongs here if you placed a generator at an angle for example, when loading this value should be saved?
	public float Orientation { get { return orientation; } protected set { orientation = value; } }

	[SerializeField]
	protected float movementCost;
	public float MovementCostMultiplier {
		get {
			if (BuildPercentage == 1) {
				return movementCost;
			} else {
				return 1;
			}
		}
	}

	// The tile additions progress towards its goal
	// For example, a door is open when the progress is 1
	// 1 == finished
	// 0 == just started
	public float Progress { get; protected set; }
	// How fast the action is performed
	// 1f = 1 second
	// 1/2 f = 2 seconds
	[SerializeField]
	protected float progressSpeed;

	public float BuildPercentage { get; protected set;}
	[SerializeField]
	protected float constructionCost;

	public TileAddition (Tile tile) : this() {
		this.tile = tile;
	}

	/// <summary>
	/// Initialize an empty tile addition
	/// Used for deserialization purpose
	/// </summary>
	public TileAddition(){
        progressSpeed = 1f;
		BuildPercentage = 0;
	}

	/// <summary>
	/// Perform work towards building the tileADdition, or performing the tile addition action.
    /// This will trigger WorkDone, as why it should be used to update the sprite (hence on performign tile addition action)
	/// </summary>
	/// <returns>The work.</returns>
	/// <param name="amount">Amount.</param>
	public virtual float DoWork(float amount){
		if (amount > 0 && BuildPercentage < 1) {
			// Build the tile if it isn't built yet
			BuildPercentage += (amount / constructionCost);
			if (BuildPercentage >= 1) {
				BuildPercentage = 1;
				if (TileAdditionBuilt != null) {
					TileAdditionBuilt (this);
				}
			}
		}
		if (WorkDone != null)
			WorkDone (this);
		else {
			Debug.LogError ("Work is being done but noone is listening?" + this.Name + " -- " + this.GetHashCode());
		}
		return BuildPercentage;
	}

	public void FinishBuilding(){
		BuildPercentage = 1;
        if (WorkDone != null)
            WorkDone(this);
    }

	// The tile should call this when it installs a piece of tile addition
	public abstract bool Conditions();
	public abstract TileAddition Clone(Tile t);

	public virtual bool DefinesRoomBorder(){
		return false;
	}

	public virtual void Update(float deltaTime) {}

	public virtual bool IsEnterable(){
		return true;
	}

	// Change the name of the object to display it's current state (can be used to render different sprites?)
	public virtual string GetRenderState(){
		return Name;
	}

    /// <summary>
    /// Cleans up the tile additions callbacks
    /// </summary>
    public void UnregisterTileAddition()
    {
        WorkDone = null;
        TileAdditionBuilt = null;

        if(TileAdditionRemoved != null)
        {
            TileAdditionRemoved(this);
        }
    }

    /// <summary>
    /// Returns true if the parent tile should be able to contain an item on the floor. For example, a wall should not be able to have an itemstack on top
    /// </summary>
    /// <param name="stack">The stack you want to put on top of the tile containing this addition</param>
    /// <returns></returns>
    public abstract bool CanContainItemOnTile(ItemStack stack);


    /// <summary>
    /// Adds the ItemStack to the tile addition if possible, by default it just returns the stack because by default TileAdditions can't contain items
    /// An Addition that can contain itemstacks has to override this method
    /// </summary>
    /// <param name="stack">the stack to add</param>
    /// <returns>The itemstack that remains after adding it to this tile addition</returns>
    public virtual ItemStack AddItemStackToTileAddition(ItemStack stack) { return stack; }

        #region Serialization
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        /// 										SERIALIZATION
        /// 
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void WriteXml (XmlWriter writer) {
		writer.WriteStartElement ("Addition");

		writer.WriteAttributeString ("TypePath", this.GetType().FullName);

		writer.WriteAttributeString ("Name", this.Name);
		writer.WriteAttributeString ("Orientation", this.orientation.ToString());
		writer.WriteAttributeString ("RenderString", this.renderString);
		writer.WriteAttributeString ("MovementCost", this.movementCost.ToString());
        writer.WriteAttributeString("BuildPercentage", this.BuildPercentage.ToString());

        writer.WriteAttributeString("Progress", this.Progress.ToString());


        WriteAdditionalXmlProperties (writer);
		writer.WriteEndElement ();
	}

	protected virtual void WriteAdditionalXmlProperties(XmlWriter writer){

	}

	public void ReadXml(XmlReader reader){
		Debug.LogError ("Trying to deserialisem a tile addition, should only hapen when called from the tile it self (and it should pass itself)");
	}

	public void ReadXml(XmlReader reader, Tile tile){
		this.tile = tile;
		this.Name = reader.GetAttribute ("Name");
		this.orientation = float.Parse (reader.GetAttribute ("Orientation"));
		this.renderString = reader.GetAttribute ("renderString");
		this.movementCost = float.Parse(reader.GetAttribute ("MovementCost"));
        this.BuildPercentage = float.Parse(reader.GetAttribute("BuildPercentage"));
        this.Progress = float.Parse(reader.GetAttribute("Progress"));

        ReadAdditionalXmlProperties (reader);
	}

	/// <summary>
	/// Reads additional properties specific to the tile addition
	/// When this is called the tile is already assigned
	/// </summary>
	/// <param name="reader">Reader.</param>
	protected virtual void ReadAdditionalXmlProperties(XmlReader reader){

	}
    #endregion
}

