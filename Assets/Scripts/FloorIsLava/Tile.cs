using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BrakingBad.Gameplay
{
    /// Attach this to each tile in the grid. The tile cracks, then disappears, after a short delay.
    public sealed class Tile : MonoBehaviour
    {
        [SerializeField] private Minigame_FloorIsLava manager;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D tileCollider;
        [SerializeField] private Color crackedTint = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private float crackDelay = 0.35f;
        [SerializeField] private float collapseDelay = 0.45f;

        private bool isCracking;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (tileCollider == null)
            {
                tileCollider = GetComponent<Collider2D>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || isCracking)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.RegisterTileTrigger(this, agent);
            }
        }

        public void BeginCrackSequence()
        {
            if (isCracking)
            {
                return;
            }

            StartCoroutine(CrackRoutine());
        }

        private IEnumerator CrackRoutine()
        {
            isCracking = true;
            yield return new WaitForSeconds(crackDelay);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = crackedTint;
            }

            yield return new WaitForSeconds(collapseDelay);

            if (tileCollider != null)
            {
                tileCollider.enabled = false;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
    }
}
