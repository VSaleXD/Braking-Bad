using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class lava : MonoBehaviour
    {
        [SerializeField] private Minigame_FloorIsLava manager;
        private Collider2D lavaCollider;

        private void Awake()
        {
            if (manager == null)
            {
                manager = FindAnyObjectByType<Minigame_FloorIsLava>();
            }

            lavaCollider = GetComponent<Collider2D>();
            if (lavaCollider != null)
            {
                lavaCollider.isTrigger = true;
                lavaCollider.enabled = false;
            }
        }
    }
}
