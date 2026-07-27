using UnityEngine;

namespace ZoneUA.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class WeaponAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip fallbackShotClip;
        [SerializeField] private AudioClip fallbackReloadClip;
        [SerializeField] private AudioClip fallbackEmptyClip;
        [SerializeField, Range(0f, 1f)] private float fallbackVolume = 0.7f;

        private void Awake()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
            }
        }

        public void PlayShot(WeaponDefinition definition)
        {
            Play(definition != null && definition.ShotClip != null ? definition.ShotClip : fallbackShotClip,
                definition != null ? definition.AudioVolume : fallbackVolume);
        }

        public void PlayReload(WeaponDefinition definition)
        {
            Play(definition != null && definition.ReloadClip != null ? definition.ReloadClip : fallbackReloadClip,
                definition != null ? definition.AudioVolume : fallbackVolume);
        }

        public void PlayEmpty(WeaponDefinition definition)
        {
            Play(definition != null && definition.EmptyClip != null ? definition.EmptyClip : fallbackEmptyClip,
                definition != null ? definition.AudioVolume : fallbackVolume);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }
    }
}
