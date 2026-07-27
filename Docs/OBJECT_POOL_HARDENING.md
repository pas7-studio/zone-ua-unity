# Runtime object pool hardening

Target editor: Unity 6 / 6000.5.5f1.

This module makes temporary runtime objects safe to reuse across projectiles, shell casings, particles, decals, popups and other short-lived effects.

## Lease generation

Every spawn creates a new logical lease for an instance. A delayed release captures the current lease generation. If the instance is returned and spawned again before the old timer completes, the old timer no longer matches and cannot release the new lease.

This prevents the common failure where a projectile or particle is reused and then unexpectedly disappears because a coroutine from its previous use completes.

## Double-release behaviour

`RuntimeObjectPool.Release` succeeds only while an instance has an active lease. A second release is ignored. `GlobalSystem.Release` distinguishes between:

- an instance owned by the pool but already returned;
- an instance not owned by the pool.

Already-returned pooled instances are not destroyed. Unknown objects keep the existing `Destroy` fallback.

## Cached reset metadata

The pool caches the following arrays once when an instance is created:

- `Rigidbody2D`;
- `Rigidbody`;
- `TrailRenderer`;
- `ParticleSystem`;
- `IPoolable` callbacks.

Spawn and release no longer call `GetComponentsInChildren<ParticleSystem>(true)` on every reuse.

The pool resets:

- 2D and 3D linear velocity;
- angular velocity;
- trail history;
- particle emission and particles;
- prefab local scale.

## Pool lifecycle contract

Components that need explicit reset logic can implement:

```csharp
public interface IPoolable
{
    void OnPoolSpawned();
    void OnPoolReleased();
}
```

`OnPoolSpawned` runs after the object becomes active. `OnPoolReleased` runs before reset and deactivation.

`AutoDispose` uses this lifecycle and schedules its lifetime through the pool-aware release scheduler.

## Capacity and prewarming

`RuntimeObjectPool.maxInactivePerPrefab` limits retained inactive objects for each prefab. Excess returned instances are destroyed and removed from tracking.

Prewarm through:

```csharp
GlobalSystem.Instance.Prewarm(prefab, count);
```

Prewarm counts should be chosen from profiler data, not guessed globally.

## Inspector and scene migration

1. Keep one `RuntimeObjectPool` under the `GlobalSystem` runtime container.
2. Set `maxInactivePerPrefab` to a conservative value such as 32–64 until profiling is available.
3. Add `AutoDispose` only to objects whose lifetime should restart on each pool spawn.
4. Implement `IPoolable` for components with state that is not reset by `OnEnable` or the built-in physics/particle reset.
5. Prewarm only frequently spawned prefabs with known burst demand.

## Validation checklist

- Open the project in Unity 6000.5.5f1 with zero compilation errors.
- Run all EditMode tests, including `PoolLeaseStateTests`.
- Spawn, release and respawn the same projectile before its original delayed lifetime ends.
- Confirm the stale timer does not disable the new projectile lease.
- Call release twice and confirm the pooled object is not destroyed.
- Confirm unknown non-pooled objects still use the Destroy fallback.
- Confirm trails and particles do not retain visuals from a previous lease.
- Confirm Rigidbody2D and Rigidbody velocities reset on reuse.
- Confirm `AutoDispose` restarts lifetime on every spawn.
- Confirm pool capacity destroys overflow without leaving invalid reusable entries.
- Profile projectile/effect-heavy scenes and tune prewarm and capacity per prefab.
