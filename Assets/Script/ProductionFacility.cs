using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZoneUA.Economy;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class ProductionFacility : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public List<ProductionQueueEntry> entries = new List<ProductionQueueEntry>();
    }

    [SerializeField] private InventoryComponent inputInventory;
    [SerializeField] private InventoryComponent outputInventory;
    [SerializeField] private List<ProductionRecipe> allowedRecipes = new List<ProductionRecipe>();
    [SerializeField] private bool processAutomatically = true;

    private readonly ProductionQueueState queue = new ProductionQueueState();

    public string ParticipantKey => "production-facility";
    public int ParticipantVersion => 1;
    public IReadOnlyList<ProductionQueueEntry> Queue => queue.entries;

    private void Awake()
    {
        inputInventory ??= GetComponent<InventoryComponent>();
        outputInventory ??= inputInventory;
    }

    private void Update()
    {
        if (processAutomatically) Tick(Time.deltaTime);
    }

    public bool Enqueue(ProductionRecipe recipe, int cycles = 1)
    {
        if (recipe == null || cycles <= 0 || !allowedRecipes.Contains(recipe) || inputInventory == null) return false;
        List<InventoryEntry> totalInputs = recipe.BuildInputs()
            .Select(entry => new InventoryEntry(entry.ItemId, entry.Amount * cycles))
            .ToList();
        if (!inputInventory.TryConsume(totalInputs)) return false;
        queue.Enqueue(recipe.RecipeId, cycles);
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (queue.entries.Count == 0) return;
        ProductionRecipe recipe = FindRecipe(queue.entries[0].recipeId);
        if (recipe == null)
        {
            queue.entries.RemoveAt(0);
            return;
        }

        InventoryComponent destination = outputInventory != null ? outputInventory : inputInventory;
        if (destination == null) return;
        List<InventoryEntry> outputs = recipe.BuildOutputs();
        int outputCount = outputs.Sum(entry => entry.Amount);
        if (destination.Capacity > 0 && destination.TotalItemCount + outputCount > destination.Capacity) return;

        if (!queue.TryAdvance(deltaTime, recipe.DurationSeconds, out _)) return;
        foreach (InventoryEntry output in outputs) destination.Add(output.ItemId, output.Amount);
    }

    public string CaptureState()
    {
        queue.Normalize();
        return JsonUtility.ToJson(new Payload { entries = queue.entries });
    }

    public void RestoreState(string payload, int version)
    {
        Payload restored = JsonUtility.FromJson<Payload>(payload ?? string.Empty);
        queue.entries = restored?.entries ?? new List<ProductionQueueEntry>();
        queue.Normalize();
    }

    private ProductionRecipe FindRecipe(string recipeId) => allowedRecipes.FirstOrDefault(recipe =>
        recipe != null && string.Equals(recipe.RecipeId, recipeId, StringComparison.Ordinal));
}
