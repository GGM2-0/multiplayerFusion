using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] NumberField HealthDisplay;
    [SerializeField] private Transform entityTransform;
    [SerializeField] private float wobbleIntensity = 0.1f;
    [SerializeField] private float wobbleSpeed = 1f;

    private float wobbleTimer = 0f;
    private Vector3 originalScale;
    private bool isWobbling = false;
    private bool firstTime = true;

    [Networked(OnChanged = nameof(NetworkedHealthChanged))]
    public int NetworkedHealth { get; set; } = 100;

    private static void NetworkedHealthChanged(Changed<Health> changed) {
        Debug.Log($"Health changed to: {changed.Behaviour.NetworkedHealth}");
        changed.Behaviour.HealthDisplay.SetNumber(changed.Behaviour.NetworkedHealth);
        if (changed.Behaviour.firstTime) {
            changed.Behaviour.originalScale = changed.Behaviour.entityTransform.localScale;
            changed.Behaviour.firstTime = false;
        } else {
            changed.Behaviour.isWobbling = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(int damage) {
        Debug.Log("Received DealDamageRpc on StateAuthority, modifying Networked variable");
        NetworkedHealth -= damage;
    }

    private void Update() {
        if (isWobbling) {
            if (wobbleTimer < wobbleSpeed) {
                wobbleTimer += Time.deltaTime;
                float normalizedTime = wobbleTimer / wobbleSpeed;
                entityTransform.localScale = Vector3.Lerp(originalScale, originalScale * (1 + wobbleIntensity), normalizedTime);
            } else if (wobbleTimer < 2 * wobbleSpeed) {
                wobbleTimer += Time.deltaTime;
                float normalizedTime = (wobbleTimer - wobbleSpeed) / wobbleSpeed;
                entityTransform.localScale = Vector3.Lerp(originalScale * (1 + wobbleIntensity), originalScale, normalizedTime);
            } else {
                wobbleTimer = 0f;
                isWobbling = false;
                entityTransform.localScale = originalScale;
            }
        }

    }
}
