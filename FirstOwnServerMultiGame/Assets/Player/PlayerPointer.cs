using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointer : MonoBehaviour
{
    protected PlyaerPointerAdmin playerPointerAdmin;

    public virtual void Initialize(PlyaerPointerAdmin playerPointerAdmin)
    {
        this.playerPointerAdmin = playerPointerAdmin;
    }

    public virtual void Invoke_detach_from_playerAdmin()
    {
        
    }
}
