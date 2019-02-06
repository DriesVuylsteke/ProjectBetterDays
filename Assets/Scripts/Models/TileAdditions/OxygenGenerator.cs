using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class OxygenGenerator : TileAddition {
	
	public static string AdditionName = "OxygenGenerator";
	float OxygenPerSecond = 5f;
	float MaxOxygen = 20f;

	public OxygenGenerator () : base() {
		SetupOxygenGenerator ();
	}

	public OxygenGenerator (Tile tile) : base (tile)
	{
		SetupOxygenGenerator ();
	}

	void SetupOxygenGenerator(){
		this.Name = OxygenGenerator.AdditionName;
		movementCost = 0; // You can't move through this tile, but it doesn't define the edge of a room
		constructionCost = 2; // It takes two seconds to construct the generator
	}

	public override bool Conditions(){
		return this.tile.TileType == TileType.Floor && // A generator requires a floor
			this.tile.Addition == null && // A generator can not be placed on another addition
			tile.Room != null; // a generator needs a room to provide oxygen to
	}

	public override TileAddition Clone(Tile t){
		OxygenGenerator w = new OxygenGenerator (t);
		return w;
	}

	public override void Update(float deltaTime){
		// Don't do anything if not fully built
		if (BuildPercentage < 1)
			return;

		tile.Room.AddOxygen (deltaTime * OxygenPerSecond, MaxOxygen);
	}

    public override bool CanContainItemOnTile(ItemStack stack)
    {
        return false;
    }

    #region Serialization
    protected override void WriteAdditionalXmlProperties (System.Xml.XmlWriter writer)
	{
		base.WriteAdditionalXmlProperties (writer);

		writer.WriteAttributeString ("OxygenPerSecond", OxygenPerSecond.ToString ());
		writer.WriteAttributeString ("MaxOxygen", MaxOxygen.ToString ());
	}

	protected override void ReadAdditionalXmlProperties (System.Xml.XmlReader reader)
	{
		base.ReadAdditionalXmlProperties (reader);

		OxygenPerSecond = float.Parse (reader.GetAttribute ("OxygenPerSecond"));
		MaxOxygen = float.Parse (reader.GetAttribute ("MaxOxygen"));
	}
#endregion

}
