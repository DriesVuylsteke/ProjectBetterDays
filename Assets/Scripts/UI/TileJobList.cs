using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileJobList : MonoBehaviour
{

    public GameObject jobListGO;
    public GameObject buttonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Opens up the gui for a character to select a job at the current tile
    /// </summary>
    /// <param name="activeCharacter"></param>
    public void Mouse1Down(Character activeCharacter, Tile tileUnderMouse)
    {

        // If we right click open up the gui
        if (activeCharacter != null)
        {
            // Clear all the children, this isn't very efficient but whatever
            // TODO: OPTIMISE
            
            jobListGO.SetActive(true);
            foreach (Transform t in jobListGO.transform)
            {
                Destroy(t.gameObject);
            }
            jobListGO.transform.position = Input.mousePosition;

            if(tileUnderMouse != null)
            {
                foreach(Job job in tileUnderMouse.jobs)
                {
                    AddButtonToActivateJob(job, activeCharacter);
                }
                MovementJob moveHere = new MovementJob(tileUnderMouse);
                AddButtonToActivateJob(moveHere, activeCharacter);
                
            }
        }
    }

    private void Update()
    {
        // If we are over a UI element, bail out
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Left clicking should disable the gui
            jobListGO.SetActive(false);
        }
    }

    private void AddButtonToActivateJob(Job job, Character characterToAssignJobTo)
    {
        GameObject go = Instantiate(buttonPrefab, jobListGO.transform);
        Text text = go.GetComponentInChildren<Text>();
        text.text = job.GetJobTitle();

        Button but = go.GetComponent<Button>();
        but.onClick.AddListener(() => {
            Debug.Log("Activating job " + job.GetJobTitle() + " for character: " + characterToAssignJobTo.GetHashCode());

            // Actually force the job
            characterToAssignJobTo.OverrideJob(job);

            jobListGO.SetActive(false);

            foreach (Transform t in jobListGO.transform)
            {
                Destroy(t.gameObject);
            }
        });
    }
}
