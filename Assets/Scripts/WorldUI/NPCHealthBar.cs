using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCHealthBar : MonoBehaviour
{

    [SerializeField] private Shader healthBarShader;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image healthBarBackgroundImage;

    [SerializeField] private Color healthBarColor;
    [SerializeField] private Color healthBarColorDepleted;
    [SerializeField] private Color healthBarBackgroundColor;

    private Material healthBarMaterial;

    private NPC npc;

    private const string HealthPercentage = "_HealthPercentage";
    private const string HealthColor = "_HealthColor";
    private const string HealthColorDepleted = "_HealthColorDepleted";



    public void Setup(NPC npc) {
        SetMaterialsAndColors();
        this.npc = npc;
        npc.npcStats.OnCurrentHealthChanged += UpdateHealthBar;
        UpdateHealthBar(this, npc.npcStats.currentHealth);
    }
    private void SetMaterialsAndColors() {
        healthBarMaterial = new Material(healthBarShader);

        healthBarImage.material = healthBarMaterial;
        healthBarMaterial.SetColor(HealthColor, healthBarColor);
        healthBarMaterial.SetColor(HealthColorDepleted, healthBarColorDepleted);
        healthBarBackgroundImage.color = healthBarBackgroundColor;

    }

    private void UpdateHealthBar(object sender, float e)
    {
        healthBarMaterial.SetFloat(HealthPercentage, npc.npcStats.GetHealthPercentage());
    }

    private void Update() {
        FacePlayerCamera();
    }

    private void FacePlayerCamera() {
        // make the health bar face the player camera while staying horizontal
        Quaternion lookRotation = PlayerCamera.Instance.transform.rotation;
        transform.rotation = lookRotation;
    }

}
