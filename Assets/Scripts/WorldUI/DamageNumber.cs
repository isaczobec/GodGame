using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float initialMoveSpeedMax = 3f;
    [SerializeField] private float initialMoveSpeedMin = 2f;
    [SerializeField] private float movementSpeedDecayMultiplier = 0.94f;

    [SerializeField] private float spawnPosRandomizationFactor = 3f;

    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float timeUntilFade = 2f;
    private float passedTime = 0;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private Transform damageNumberTransform;



    [SerializeField] private Color playerTookDamageColor;
    [SerializeField] private Color enemyTookDamageColor;

    private Vector3 velocity;

    public void Setup(Vector3 worldPosition, float damage, bool enemyTookDamage, float scale)
    {
        damageNumberTransform.localScale = damageNumberTransform.localScale * scale; 
        transform.position = worldPosition + offset;
        SetDamage(damage);
        Destroy(gameObject, lifeTime);

        float randomX = Random.Range(initialMoveSpeedMin, initialMoveSpeedMax) * (Random.Range(0, 2) == 0 ? -1 : 1);
        float randomZ = Random.Range(initialMoveSpeedMin, initialMoveSpeedMax) * (Random.Range(0, 2) == 0 ? -1 : 1);
        velocity = new Vector3(randomX, 2.5f * Random.Range(initialMoveSpeedMin, initialMoveSpeedMax), randomZ); // y should always be positive

        float randomXPos = Random.Range(0, spawnPosRandomizationFactor) * (Random.Range(0, 2) == 0 ? -1 : 1);
        float randomYPos = Random.Range(0, spawnPosRandomizationFactor) * (Random.Range(0, 2) == 0 ? -1 : 1);
        float randomZPos = Random.Range(0, spawnPosRandomizationFactor) * (Random.Range(0, 2) == 0 ? -1 : 1);
        transform.position += new Vector3(randomXPos, randomYPos, randomZPos);

        textMesh.color = enemyTookDamage ? enemyTookDamageColor : playerTookDamageColor;
    }

    private void Update()
    {
        passedTime += Time.deltaTime;
        // update position
        transform.position += velocity * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, Vector3.zero, movementSpeedDecayMultiplier * Time.deltaTime);

        // make the health bar face the player camera while staying horizontal
        Quaternion lookRotation = PlayerCamera.Instance.transform.rotation;
        transform.rotation = lookRotation;

        if (passedTime > timeUntilFade)
        {
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1 - (passedTime - timeUntilFade) / (lifeTime - timeUntilFade));
        }
    }

    public void SetDamage(float damage)
    {
        textMesh.text = damage.ToString();
    }
}
