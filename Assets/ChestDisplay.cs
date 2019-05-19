using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestDisplay : MonoBehaviour
{
    [SerializeField]
    protected Image displayImage;
    [SerializeField]
    protected Text valueText;

    protected ItemStack stackToDisplay;

    public void SetStackToDisplay(ItemStack stack)
    {
        Debug.Log("Stack to display:" + stack);
        if(stackToDisplay != null)
        {
            stackToDisplay.StackCountUpdated -= StackCountUpdated;
        }
        stackToDisplay = stack;
        Sprite s = TileSpriteController.GetSprite("Item_" + stack.GetStackType());
        displayImage.sprite = s;
        StackCountUpdated(stack);
        stack.StackCountUpdated += StackCountUpdated;
    }

    private void StackCountUpdated(ItemStack stack)
    {
        valueText.text = ""+stack.GetCurrentStackSize();
    }
}
