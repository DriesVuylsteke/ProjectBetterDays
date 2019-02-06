using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class ItemFactory
{
    // Creates a tomato and returns it
    public static  Item GetTomato()
    {
        return new Item()
        {
            ItemName = ItemValues.tomato_name,
            MaxStackSize = ItemValues.tomato_max_stackSize
        };
    }
}
