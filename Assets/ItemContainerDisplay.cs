using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to display an itemcontainer on the appropriate gui
public class ItemContainerDisplay : MonoBehaviour
{
    [SerializeField]
    protected ChestDisplay[] displayIcons;
    protected ItemContainer currentChest;

    public void SetContainer(ItemContainer container)
    {
        if(currentChest != null)
        {
            currentChest.OnStacksModified -= ContainerModified;
        }
        currentChest = container;
        ContainerModified(container.GetStoredItemStacks());
        currentChest.OnStacksModified += ContainerModified;
    }

    // Update the gui
    private void ContainerModified(ItemStack[] itemStacks)
    {
        
        if (itemStacks.Length > displayIcons.Length)
        {
            Debug.LogError("the container has more stacks than the display can handle");
            return;
        }
        int current = 0;
        
        while (current < itemStacks.Length )
        {
            Debug.Log(itemStacks[current]);
            displayIcons[current].gameObject.SetActive(true);
            displayIcons[current].SetStackToDisplay(itemStacks[current]);
            current++;
        } 

        while(current < displayIcons.Length)
        {
            displayIcons[current].gameObject.SetActive(false);
            current++;
        }
    }
}
