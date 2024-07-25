using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{

    [SerializeField] private GameObject damageNumberPrefab;


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
        damageNumberGameObject.transform.parent = transform;
        DamageNumber damageNumber = damageNumberGameObject.GetComponent<DamageNumber>();
        damageNumber.Setup(position, damage, enemyTookDamage);

    }
}
