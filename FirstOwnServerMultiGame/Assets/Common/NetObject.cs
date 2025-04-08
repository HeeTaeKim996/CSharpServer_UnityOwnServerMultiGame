using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class NetObject : MonoBehaviour
{
    public NetObjectCode netObjectCode { get; protected set; }
    public short id { get; protected set; }
}
