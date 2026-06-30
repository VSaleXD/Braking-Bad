using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Persistent background music manager. Mengikuti pola yang sama dengan TournamentManager:
    /// satu instance hidup sepanjang sesi game lewat DontDestroyOnLoad, musiknya TIDAK ikut
    /// restart/reset setiap pindah scene, kecuali memang diminta ganti track.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Default Music")]
        [SerializeField] private AudioClip defaultMusic;
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.6f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnAwake = true;

        [Header("Fade")]
        [SerializeField] private float fadeDuration = 0.5f;

        private AudioSource audioSource;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = loop;
            audioSource.playOnAwake = false;
            audioSource.volume = defaultVolume;

            if (playOnAwake && defaultMusic != null && !audioSource.isPlaying)
            {
                audioSource.clip = defaultMusic;
                audioSource.Play();
            }
        }

        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }

            if (audioSource.clip == clip && audioSource.isPlaying)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = fade
                ? StartCoroutine(CrossfadeTo(clip))
                : StartCoroutine(SwitchImmediately(clip));
        }

        public void StopMusic(bool fade = true)
        {
            if (audioSource == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = fade ? StartCoroutine(FadeOutAndStop()) : null;

            if (!fade)
            {
                audioSource.Stop();
            }
        }

        public void SetVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
            }
        }

        private System.Collections.IEnumerator CrossfadeTo(AudioClip newClip)
        {
            float startVolume = audioSource.volume;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }

            audioSource.clip = newClip;
            audioSource.Play();

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(0f, startVolume, elapsed / fadeDuration);
                yield return null;
            }

            audioSource.volume = startVolume;
        }

        private System.Collections.IEnumerator SwitchImmediately(AudioClip newClip)
        {
            audioSource.clip = newClip;
            audioSource.Play();
            yield break;
        }

        private System.Collections.IEnumerator FadeOutAndStop()
        {
            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }
    }
}