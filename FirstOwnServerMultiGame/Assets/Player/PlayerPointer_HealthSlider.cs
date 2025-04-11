using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerPointer_HealthSlider : PlayerPointer
{
    private LivingEntity attachingEntity;
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Slider easeHealthSlider;
    private Coroutine sliderCorouitne;

    private void Awake()
    {
        healthSlider.minValue = 0f;
        easeHealthSlider.minValue = 0f;
    }

    public override void Initialize(PlyaerPointerAdmin playerPointerAdmin)
    {
        base.Initialize(playerPointerAdmin);
    }

    public void Attach(LivingEntity entity)
    {
        attachingEntity = entity;
        entity.Event_invoke_detach_from_entity += On_invoekd_detach_from_entity;
        entity.Event_on_attached_entitys_GetDamaged += On_attachingEntity_getDamage;

        if(sliderCorouitne != null)
        {
            StopCoroutine(sliderCorouitne);
            sliderCorouitne = null;
        }
        healthSlider.maxValue = entity.maxHealth;
        healthSlider.value = entity.health;
        easeHealthSlider.maxValue = entity.maxHealth;
        easeHealthSlider.value = entity.health;
    }


    private void LateUpdate()
    {
        transform.position = attachingEntity.transform.position;
        transform.rotation = Quaternion.Euler(new Vector3(-90, 180, 0));
    }

    private void On_attachingEntity_getDamage()
    {
        if (sliderCorouitne != null)
        {
            StopCoroutine(sliderCorouitne);
        }
        sliderCorouitne = StartCoroutine(SliderCoroutine_onDamage(attachingEntity.health));
    }
    private IEnumerator SliderCoroutine_onDamage(float health)
    {
        healthSlider.value = health;
        float minMinusValue = (float)attachingEntity.maxHealth * 0.01f;

        while(easeHealthSlider.value > health)
        {
            float minusValue = (float)(easeHealthSlider.value - health) * Time.deltaTime * 5f; 
            easeHealthSlider.value = minMinusValue > minusValue ? easeHealthSlider.value -= minMinusValue : easeHealthSlider.value -= minusValue;

            yield return null;
        }
        easeHealthSlider.value = health;

        sliderCorouitne = null;
    }


    private void On_invoekd_detach_from_entity()
    {
        attachingEntity.Event_invoke_detach_from_entity -= On_invoekd_detach_from_entity;
        attachingEntity.Event_on_attached_entitys_GetDamaged -= On_attachingEntity_getDamage;
        attachingEntity = null;
        playerPointerAdmin.Get_back_from_pointer(this);
    }
    public override void Invoke_detach_from_playerAdmin()
    {
        base.Invoke_detach_from_playerAdmin();
        attachingEntity.Event_invoke_detach_from_entity -= On_invoekd_detach_from_entity;
        attachingEntity.Event_on_attached_entitys_GetDamaged -= On_attachingEntity_getDamage;
        attachingEntity = null;
    }

    public bool is_same_enity(LivingEntity entity)
    {
        return attachingEntity = entity;
    }
    public void Switch_attaching_entity(LivingEntity entity)
    {
        attachingEntity.Event_invoke_detach_from_entity -= On_invoekd_detach_from_entity;
        attachingEntity.Event_on_attached_entitys_GetDamaged -= On_attachingEntity_getDamage;
        Attach(entity);
    }
}
