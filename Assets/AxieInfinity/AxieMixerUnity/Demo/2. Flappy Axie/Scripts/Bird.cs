using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Bird : MonoBehaviour
    {
        private AxieFigure figure;
        public float velocity = 2.4f;

        private void Start()
        {
            figure = gameObject.GetComponentInChildren<AxieFigure>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.up * velocity;
                figure?.DoJumpAnim();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                figure?.DoAttackMeleeAnim();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                figure?.DoAttackRangedAnim();
            }
        }

        private void OnCollisionEnter2D(Collision2D coll)
        {
            // Restart
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
