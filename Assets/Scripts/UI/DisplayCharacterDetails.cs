using UnityEngine;
using UnityEngine.UI;

public class DisplayCharacterDetails : MonoBehaviour
{
    protected Character currentCharacter;
    World world { get { return WorldController.instance.world; } }

    [SerializeField]
    protected ToggleActionGroups groups;

    [SerializeField]
    protected Text speed, construction, planting, harvesting;

    protected Character curCharacter;

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
    }

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
        } else
        {
            // Hide the character menu
            groups.DisableCharacterGUI();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
