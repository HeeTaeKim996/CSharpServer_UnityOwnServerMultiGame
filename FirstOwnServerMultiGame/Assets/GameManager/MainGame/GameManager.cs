using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using FreeNet;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public event Action event_lateStart;

    [SerializeField]
    private Transform spawnPivot;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        event_lateStart += late_start;
    }
    private void Start()
    {
        
    }
    public void Invoke_lateStart()
    {
        event_lateStart?.Invoke();
    }
    private void late_start()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle;
        Vector3 spawnPosition = new Vector3(spawnPivot.position.x + randomCircle.x * 5f, spawnPivot.position.y, spawnPivot.position.z + randomCircle.y * 5f);

        CommonMethods.Instantiate_netObject(CNetworkManager.instance.room_id, NetObjectCode.Player, spawnPosition, Vector3.zero);

        Debug.Log($"GameMAnaer DebugCHeck __ room_id : {CNetworkManager.instance.room_id}, NetObjectCode : {NetObjectCode.Player}, spawnPosition : {spawnPosition}, rotation : {Vector3.zero}");
    }







}
