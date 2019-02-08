using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCharacterDetails : MonoBehaviour
{
    protected Character currentCharacter;
    World world { get { return WorldController.instance.world; } }

    [SerializeField]
    protected ToggleActionGroups groups;

    [Header("Different tabs")]
    #region GUI overview
    [SerializeField]
    protected GameObject stats;
    [SerializeField]
    protected GameObject workPriority;
    #endregion GUI overview

    [Header("Stats tab")]
    #region stats
    [SerializeField]
    protected Text speed;
    [SerializeField]
    protected Text construction;
    [SerializeField]
    protected Text planting;
    [SerializeField]
    protected Text harvesting;

    protected Character curCharacter;
    #endregion stats

    [Header("Work tab")]
    #region WorkPriorities properties
    [SerializeField]
    GameObject workButtonsParent;

    [SerializeField]
    GameObject workButtonPrefab;

    Dictionary<string, GameObject> prioritiesGODict;
    #endregion WorkPriorities properties

    

    // Start is called before the first frame update
    void Start()
    {
        world.OnCharacterAdded += CharacterAdded;
        if(world.GetCharacters().Count != 0)
        {
            foreach(Character c in world.GetCharacters())
            {
                CharacterAdded(c);
            }
        }

        #region workPriorities
        prioritiesGODict = new Dictionary<string, GameObject>();

        Debug.Log(world.Jobs.GetActiveQueues().Count + " is the amount of job queues");
        foreach (string jobPriority in world.Jobs.GetActiveQueues())
        {
            Jobs_JobQueueAdded(null, jobPriority);
        }

        world.Jobs.JobQueueAdded += Jobs_JobQueueAdded;
        #endregion workPriorities
    }

    #region workPriorities

    private void Jobs_JobQueueAdded(JobQueue theQueue, string addedQueue)
    {
        Debug.Log("Job priority button created");
        GameObject go = Instantiate(workButtonPrefab, workButtonsParent.transform);
        Text text = go.GetComponentInChildren<Text>();
        text.text = addedQueue;

        Button but = go.GetComponent<Button>();
        but.onClick.AddListener(() => SwapPriorities(addedQueue));

        prioritiesGODict.Add(addedQueue, go);
    }

    string firstPriority = "";
    private void SwapPriorities(string jobPriorityChange)
    {
        
        if (firstPriority.Equals(""))
        {
            firstPriority = jobPriorityChange;
        } else
        {
            List<string> oldPriorities = curCharacter.jobPriorities;
            
            int firstIndex = oldPriorities.IndexOf(firstPriority);
            int secondIndex = oldPriorities.IndexOf(jobPriorityChange);

            curCharacter.SetJobPriority(jobPriorityChange, firstIndex);
            curCharacter.SetJobPriority(firstPriority, secondIndex);
            firstPriority = "";

            OrganisePriorities(); // Update the visual representation
        }
    }

    /// <summary>
    /// Organises the visualisation of the priorities to represent the actual priorities
    /// </summary>
    private void OrganisePriorities()
    {
        List<string> jobPriorities = curCharacter.jobPriorities;
        for (int i = 0; i < jobPriorities.Count; i++)
        {
            prioritiesGODict[jobPriorities[i]].transform.SetSiblingIndex(i);
        }
    }

    #endregion workPriorities

    #region DisplaySpecificGUI

    public void DisplayStats()
    {
        stats.SetActive(true);
        workPriority.SetActive(false);
    }

    public void DisplayWorkPriorities()
    {
        stats.SetActive(false);
        workPriority.SetActive(true);
    }

    #endregion DisplaySpecificGUI

    #region CharacterSelection
    void CharacterAdded(Character character)
    {
        Debug.Log("Character added");
        character.OnCharacterSelectedChanged += CharacterSelectionChanged;
        CharacterSelectionChanged(character);
    }

    public void DeselectCharacter()
    {
        if(curCharacter != null)
        {
            curCharacter.DeselectCharacter();
            firstPriority = "";
        }
    }

    void CharacterSelectionChanged(Character character)
    {
        if (character.Selected)
        {
            // Display panel
            groups.ToggleCharacter();
            // Display information
            speed.text = character.Speed.ToString();
            construction.text = character.Construction.ToString();
            planting.text = character.Planting.ToString();
            harvesting.text = character.Harvesting.ToString();
            curCharacter = character;
            OrganisePriorities();
        } else
        {
            // Hide the character menu
            groups.DisableCharacterGUI();
        }
    }
    #endregion CharacterSelection

    // Update is called once per frame
    void Update()
    {
        
    }
}
