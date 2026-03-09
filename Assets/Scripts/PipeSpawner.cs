using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject pipePairSource;
    [SerializeField] private Camera gameplayCamera;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float firstSpawnDelay = 1f;

    [Header("Vertical Range (Pipe Center Y)")]
    [SerializeField] private float minY = -1f;
    [SerializeField] private float maxY = 1f;
    [SerializeField] private bool clampInsideCamera = true;
    [SerializeField] private float cameraPadding = 0.5f;

    [Header("Options")]
    [SerializeField] private bool disableSourceOnStart = true;

    private float timer;
    private float sourceHalfHeight = 2f;

    private void Start()
    {
        timer = -firstSpawnDelay;

        CacheSourceBounds();

        if (disableSourceOnStart && pipePairSource != null)
        {
            pipePairSource.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnPipe();
            timer = 0f;
        }
    }

    private void SpawnPipe()
    {
        if (pipePairSource == null)
        {
            return;
        }

        float spawnCenterY = GetSpawnCenterY();
        GameObject spawned = Instantiate(pipePairSource, pipePairSource.transform.position, Quaternion.identity);

        if (TryGetBounds(spawned, out Bounds spawnedBounds))
        {
            Vector3 delta = new Vector3(transform.position.x - spawnedBounds.center.x, spawnCenterY - spawnedBounds.center.y, 0f);
            spawned.transform.position += delta;
        }
        else
        {
            spawned.transform.position = new Vector3(transform.position.x, spawnCenterY, 0f);
        }

        spawned.SetActive(true);
    }

    private float GetSpawnCenterY()
    {
        float y = Random.Range(minY, maxY);

        if (!clampInsideCamera)
        {
            return y;
        }

        Camera cam = gameplayCamera != null ? gameplayCamera : Camera.main;
        if (cam == null || !cam.orthographic)
        {
            return y;
        }

        float camMinY = cam.transform.position.y - cam.orthographicSize;
        float camMaxY = cam.transform.position.y + cam.orthographicSize;
        float minAllowedY = camMinY + sourceHalfHeight + cameraPadding;
        float maxAllowedY = camMaxY - sourceHalfHeight - cameraPadding;

        if (minAllowedY > maxAllowedY)
        {
            return (camMinY + camMaxY) * 0.5f;
        }

        return Mathf.Clamp(y, minAllowedY, maxAllowedY);
    }

    private void CacheSourceBounds()
    {
        if (pipePairSource == null)
        {
            return;
        }

        if (TryGetBounds(pipePairSource, out Bounds bounds))
        {
            sourceHalfHeight = bounds.extents.y;
        }
    }

    private static bool TryGetBounds(GameObject target, out Bounds bounds)
    {
        Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>(true);
        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return true;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        bounds = default;
        return false;
    }
}
