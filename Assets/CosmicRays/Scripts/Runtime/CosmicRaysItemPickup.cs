using UnityEngine;

/// <summary>Collects an assigned item definition when the configured player enters its trigger.</summary>
[RequireComponent(typeof(Collider))]
public sealed class CosmicRaysItemPickup : MonoBehaviour
{
    [SerializeField] private InventoryItemDefinition item;
    [SerializeField, Min(1)] private int quantity = 1;
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        Collider itemCollider = GetComponent<Collider>();
        itemCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        quantity = Mathf.Max(1, quantity);
        if (string.IsNullOrWhiteSpace(playerTag))
        {
            playerTag = "Player";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (item == null)
        {
            Debug.LogError("Item pickup has no InventoryItemDefinition assigned.", this);
            return;
        }
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        CosmicRaysGameplayController controller = CosmicRaysGameplayController.Active;
        if (controller == null)
        {
            Debug.LogError("Item pickup was touched while no COSMIC RAYS gameplay controller is active.", this);
            return;
        }

        int added = controller.CollectItem(item, quantity);
        if (added <= 0)
        {
            return;
        }

        quantity -= added;
        if (quantity <= 0)
        {
            Destroy(gameObject);
        }
    }
}
