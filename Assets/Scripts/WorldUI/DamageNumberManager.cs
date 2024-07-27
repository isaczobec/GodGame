using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{

    [SerializeField] private GameObject damageNumberPrefab;

    [SerializeField] private float minDamageNumberScaleFactor = 0.5f;
    [SerializeField] private float maxDamageNumberScaleFactor = 3f;

    private float damageAverage = 20f;
    private int amountOfDamagesInAverage = 10;


    public static DamageNumberManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        
    }
    
    public void CreateDamageNumber(Vector3 position, float damage, bool enemyTookDamage)
    {
        GameObject damageNumberGameObject = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        damageNumberGameObject.transform.SetParent(transform,worldPositionStays: false);
        DamageNumber damageNumber = damageNumberGameObject.GetComponent<DamageNumber>();

        // calculate the scale based on previous damage
        float scale = Mathf.Clamp(damage / damageAverage, minDamageNumberScaleFactor, maxDamageNumberScaleFactor);
        UpdateAverage(damage);

        damageNumber.Setup(position, damage, enemyTookDamage, scale);

    }

    public void UpdateAverage(float damage) {
        damageAverage *= 1-(1/(float)amountOfDamagesInAverage);
        damageAverage += damage/(float)amountOfDamagesInAverage;
    }
}
