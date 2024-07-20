using UnityEngine;

public class NPCAnimationEventsTransmitter : MonoBehaviour
{
    [SerializeField] private NPCvisual NPCvisual;

    public void OnActionPerformed() {
        NPCvisual.OnAnimationActionPerformed();
    }

    public void OnActionEnded() {
        NPCvisual.OnAnimationActionEnded();

        Debug.Log("ENDED");
    }
}