using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PlyaerPointerAdmin : MonoBehaviour
{
    private PlayerPointer usingPointer;
    [SerializeField]
    private PlayerPointer_HealthSlider healthPointer_prefab;
    private PlayerPointer_HealthSlider healthPointer;

    private void Awake()
    {
        healthPointer = Instantiate(healthPointer_prefab);
        healthPointer.Initialize(this);
    }
    private void Start()
    {
        healthPointer.gameObject.SetActive(false);
    }

    public void Attach_healthSlider(LivingEntity entity)
    {
        if(usingPointer != null)
        {
            if(usingPointer == healthPointer)
            {
                if (healthPointer.is_same_enity(entity))
                {
                    return;
                }
                else
                {
                    healthPointer.Switch_attaching_entity(entity);
                    return;
                }
            }
            else
            {
                usingPointer.Invoke_detach_from_playerAdmin();
                usingPointer.gameObject.SetActive(false);
            }
        }

        healthPointer.gameObject.SetActive(true);
        healthPointer.Attach(entity);
        usingPointer = healthPointer;
    }

    public void Get_back_from_pointer(PlayerPointer pointer)
    {
        pointer.gameObject.SetActive(false);
        usingPointer = null;
    }
    public void Retrieve_pointer()
    {
        if(usingPointer != null)
        {
            usingPointer.Invoke_detach_from_playerAdmin();
            usingPointer.gameObject.SetActive(false);
            usingPointer = null;
        }
    }
}
