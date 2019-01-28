using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGui : MonoBehaviour
{
    private bool state;
    [SerializeField]
    protected GameObject itemToToggle;

    public void Toggle()
    {
        state = !state;
        itemToToggle.SetActive(state);
    }

    public void TurnOff()
    {
        state = false;
        itemToToggle.SetActive(false);
    }

    public void TurnOn()
    {
        state = true;
        itemToToggle.SetActive(true);
    }
}
