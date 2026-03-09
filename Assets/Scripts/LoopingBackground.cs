using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private float resetX = -24f;
    [SerializeField] private float startX = 24f;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        if (transform.position.x <= resetX)
        {
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }
    }
}
