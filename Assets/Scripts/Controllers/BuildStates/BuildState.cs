using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildState {

	protected World world { get { return WorldController.instance.world; } }

	protected MouseController mouseController;
	protected BuildController buildController;



	public BuildState(MouseController mouseController, BuildController controller){
		this.mouseController = mouseController;
		this.buildController = controller;
	}

	public abstract void MouseButton0Down();
	public abstract void MouseButton0Up();
	public abstract void MouseButton0Hold();

	public virtual void MouseButton1Down(){
		buildController.State = null;
	}

	public abstract string GetTooltipSpriteName ();
}
