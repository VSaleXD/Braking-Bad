using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class SceneMusicSetter : MonoBehaviour
    {
        [SerializeField] private AudioClip musicForThisScene;
        [SerializeField] private bool fadeTransition = true;
        [SerializeField] private bool loop = true;
        private void Start()
        {
            if (musicForThisScene == null)
            {
                Debug.LogWarning($"{nameof(SceneMusicSetter)} di scene '{gameObject.scene.name}' tidak punya musicForThisScene yang di-assign.");
                return;
            }

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning($"{nameof(SceneMusicSetter)}: AudioManager.Instance belum ada. Pastikan scene Menu/Bootstrap sudah pernah dijalankan duluan.");
                return;
            }

            AudioManager.Instance.PlayMusic(musicForThisScene, fadeTransition);
            setLoop(loop);
        }
        private void setLoop(bool loop)
        {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.SetLoop(loop);
        }
    }

}