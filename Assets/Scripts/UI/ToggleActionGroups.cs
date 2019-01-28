using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActionGroups : MonoBehaviour
{
    [SerializeField]
    protected GameObject build, character;
    [SerializeField]
    protected DisplayCharacterDetails characterDetails;

    public void ToggleBuild()
    {
        ToggleActive(build);
    }

    public void ToggleCharacter()
    {
        if (!ToggleActive(character))
        {
            characterDetails.DeselectCharacter();
        }
    }

    public void DisableCharacterGUI()
    {
        character.SetActive(false);
    }

    public bool ToggleActive(GameObject go)
    {
        bool curState = go.activeSelf;
        DeactivateAll();
        go.SetActive(!curState);
        return !curState;
    }

    public void DeactivateAll()
    {
        build.SetActive(false);
        if (character.activeSelf)
        {
            characterDetails.DeselectCharacter();
        }
        character.SetActive(false);
    }
}
