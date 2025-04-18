using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSliderAdmin : MonoBehaviour
{
    private PlayerHealth playerHealth;
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Slider easeHealthSlider;
    private Coroutine sliderCorouitne;

    private void Awake()
    {
        healthSlider.minValue = 0;
        easeHealthSlider.minValue = 0;
    }

    public void Get_player(PlayerHealth playerHealth)
    {
        this.playerHealth = playerHealth;
        playerHealth.Event_invoke_detach_from_entity += On_die;
        playerHealth.Event_on_attached_entitys_GetDamaged += On_damage;
        playerHealth.Event_on_RestoreHealth += On_restore;

        healthSlider.maxValue = playerHealth.maxHealth;
        healthSlider.value = playerHealth.health;
        easeHealthSlider.maxValue = playerHealth.maxHealth;
        easeHealthSlider.value = playerHealth.health;
    }
    private void On_damage()
    {
        if (sliderCorouitne != null)
        {
            StopCoroutine(sliderCorouitne);
        }
        sliderCorouitne = StartCoroutine(OnDamagecoroutine(playerHealth.health));

    }
    private IEnumerator OnDamagecoroutine(float health)
    {
        healthSlider.value = health;
        float minMinusValue = (float)playerHealth.maxHealth * 0.001f;

        while (easeHealthSlider.value > health)
        {
            float minusValue = (float)(easeHealthSlider.value - health) * Time.deltaTime * 2f;
            easeHealthSlider.value = minMinusValue > minusValue ? easeHealthSlider.value -= minMinusValue : easeHealthSlider.value -= minusValue;

            yield return null;
        }
        easeHealthSlider.value = health;

        sliderCorouitne = null;
    } 
    private void On_restore()
    {
        if(sliderCorouitne != null)
        {
            StopCoroutine(sliderCorouitne);
        }
        sliderCorouitne = StartCoroutine(RestoreCoroutine(playerHealth.health));
    }
    private IEnumerator RestoreCoroutine(float health)
    {
        easeHealthSlider.value = healthSlider.value;
        float minPlusValue = (float)playerHealth.maxHealth * 0.001f;

        while(healthSlider.value < health)
        {
            float plusValue = (float)(health - healthSlider.value) * Time.deltaTime * 2f;
            if(minPlusValue > plusValue)
            {
                healthSlider.value += minPlusValue;
                easeHealthSlider.value += minPlusValue;
            }
            else
            {
                healthSlider.value += plusValue;
                easeHealthSlider.value += plusValue;
            }

            yield return null;
        }
        healthSlider.value = health;
        easeHealthSlider.value = health;

        sliderCorouitne = null;
    }
    private void On_die()
    {

    }
}
