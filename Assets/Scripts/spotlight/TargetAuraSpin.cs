using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Spins the target aura around the target player.
    public class TargetAuraSpin : MonoBehaviour
    {
        [SerializeField] private float spinSpeed = 60f; 

        void Update()
        {
            transform.Rotate(0f,0f, spinSpeed * Time.deltaTime);
        }
    }
}