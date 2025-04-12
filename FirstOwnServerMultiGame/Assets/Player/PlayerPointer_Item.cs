using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointer_Item : PlayerPointer
{
    public Item attachingItem;

    public override void Initialize(PlyaerPointerAdmin playerPointerAdmin)
    {
        base.Initialize(playerPointerAdmin);
    }

    public override void Invoke_detach_from_playerAdmin()
    {
        base.Invoke_detach_from_playerAdmin();
        attachingItem.Event_on_destroy -= On_item_destroyed;
        attachingItem = null;
    }

    public void Attach(Item item)
    {
        attachingItem = item;
        item.Event_on_destroy += On_item_destroyed;
    }

    private void LateUpdate()
    {
        transform.position = attachingItem.transform.position;
    }

    private void On_item_destroyed()
    {
        attachingItem.Event_on_destroy -= On_item_destroyed;
        attachingItem = null;
        playerPointerAdmin.Get_back_from_pointer(this);
    }
    
    public bool is_same_item(Item item)
    {
        return attachingItem = item;
    }

    public void Switch_attaching_item(Item item)
    {
        attachingItem.Event_on_destroy -= On_item_destroyed;
        Attach(item);
    }
}
