using System;
using UnityEngine;
using ZoneUA.Persistence;

[DisallowMultipleComponent]
[RequireComponent(typeof(PersistentIdentity))]
public sealed class TransformSaveParticipant : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class TransformState
    {
        public Vector3 position;
        public float rotationZ;
        public Vector3 localScale = Vector3.one;
        public bool active = true;
    }

    [SerializeField] private bool persistScale;
    [SerializeField] private bool persistActiveState = true;

    public string ParticipantKey => "transform";
    public int ParticipantVersion => 1;

    public string CaptureState()
    {
        var state = new TransformState
        {
            position = transform.position,
            rotationZ = transform.eulerAngles.z,
            localScale = transform.localScale,
            active = gameObject.activeSelf
        };
        return JsonUtility.ToJson(state);
    }

    public void RestoreState(string payload, int version)
    {
        if (string.IsNullOrWhiteSpace(payload)) return;
        TransformState state = JsonUtility.FromJson<TransformState>(payload);
        if (state == null) return;
        transform.SetPositionAndRotation(state.position, Quaternion.Euler(0f, 0f, state.rotationZ));
        if (persistScale) transform.localScale = state.localScale;
        if (persistActiveState) gameObject.SetActive(state.active);
    }
}