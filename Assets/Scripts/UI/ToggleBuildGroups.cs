using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBuildGroups : MonoBehaviour
{
    [SerializeField]
    protected GameObject plants, structure, commands;
    protected bool state;

    public void ToggleStructure()
    {
        AllOff();
        structure.SetActive(ToggleState());
    }

    public void TogglePlants()
    {
        AllOff();
        plants.SetActive(ToggleState());
    }

    public void ToggleCommands()
    {
        AllOff();
        commands.SetActive(ToggleState());
    }

    protected bool ToggleState()
    {
        state = !state;
        return state;
    } 

    protected void AllOff()
    {
        plants.SetActive(false);
        structure.SetActive(false);
        commands.SetActive(false);
        state = false;
    }
}
