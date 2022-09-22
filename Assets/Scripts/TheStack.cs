using UnityEngine;

public class TheStack : MonoBehaviour
{
    public Color32[] gameColors = new Color32[4];
    public Material stackMat;

    private const float BOUND_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5f;
    private const float ERROR_MARGIN = .1f;
    private const float STACK_BOUNDS_GAIN = .25f;
    private const float COMBO_START_GAIN = 3f;

    private GameObject[] theStack;
    private Vector2 stackBounds = new Vector2(BOUND_SIZE, BOUND_SIZE);

    private int stackIndex;
    private int scoreCount = 0;
    private int combo = 0;
    [SerializeField] private ScoreText scoreText;
    [SerializeField] AudioSource audioSource;

    private float tileTransition = 0;
    private float tileSpeed = 2.5f;
    private float secondaryPos;

    private bool isMovingX = true;
    private bool gameOver = false;

    private Vector3 desiredPos;
    private Vector3 lastTilePos;

    private void Start()
    {
        theStack = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
        }
        stackIndex = transform.childCount - 1;
    }

    private void Update()
    {
        if (gameOver)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                scoreCount++;
                scoreText.AddScore(scoreCount.ToString());
                audioSource.Play();
            }
            else
            {
                // end game
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
        MoveTile();
        transform.position = Vector3.Lerp(transform.position, desiredPos, STACK_MOVING_SPEED * Time.deltaTime);
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];

        float f = Mathf.Sin(scoreCount * .25f);
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);
        }

        mesh.colors32 = colors;
    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < .33f)
        {
            return Color.Lerp(a, b, t / .33f);
        }
        else if (t < .66f)
        {
            return Color.Lerp(b, c, (t - .33f) / .33f);
        }
        else
        {
            return Color.Lerp(c, d, (t - .66f) / .66f);
        }
    }

    private void MoveTile()
    {
        tileTransition += Time.deltaTime * tileSpeed;

        if (isMovingX)
        {
            theStack[stackIndex].transform.localPosition =
                new Vector3(Mathf.Sin(tileTransition) * BOUND_SIZE, scoreCount, secondaryPos);
        }
        else
        {
            theStack[stackIndex].transform.localPosition =
                new Vector3(secondaryPos, scoreCount, Mathf.Sin(tileTransition) * BOUND_SIZE);
        }
    }

    private bool PlaceTile()
    {
        Transform t = theStack[stackIndex].transform;

        if (isMovingX)
        {
            float deltaX = lastTilePos.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);

                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePos.x + t.localPosition.x / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                CreateRubble(
                    new Vector3((t.position.x > 0) ? t.position.x + (t.localScale.x / 2f) : t.position.x - (t.localScale.x / 2f),
                    t.position.y,
                    t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1f, t.localScale.z)
                    );

                t.localPosition = new Vector3(middle - (lastTilePos.x / 2f), scoreCount, lastTilePos.z);
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.x += COMBO_START_GAIN;
                    if (stackBounds.x > BOUND_SIZE)
                        stackBounds.x = BOUND_SIZE;
                    float middle = lastTilePos.x + t.localPosition.x / 2f;

                    t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePos.x / 2f), scoreCount, lastTilePos.z);
                }

                combo++;
                t.localPosition = new Vector3(lastTilePos.x, scoreCount, lastTilePos.z);
            }
        }
        else
        {
            float deltaZ = lastTilePos.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePos.z + t.localPosition.z / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                CreateRubble(
                    new Vector3(t.position.x,
                    t.position.y,
                    (t.position.z > 0) ? t.position.z + (t.localScale.z / 2f) : t.position.z - (t.localScale.z / 2f)),
                    new Vector3(t.localScale.x, 1f, Mathf.Abs(deltaZ))
                    );

                t.localPosition = new Vector3(lastTilePos.x / 2f, scoreCount, middle - (lastTilePos.z / 2f));
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.y += COMBO_START_GAIN;
                    if (stackBounds.y > BOUND_SIZE)
                        stackBounds.y = BOUND_SIZE;
                    float middle = lastTilePos.z + t.localPosition.z / 2f;

                    t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePos.x / 2f, scoreCount, middle - (lastTilePos.z / 2f));
                }

                combo++;
                t.localPosition = new Vector3(lastTilePos.x, scoreCount, lastTilePos.z);
            }
        }

        secondaryPos = (isMovingX) ? t.localPosition.x : t.localPosition.z;
        isMovingX = !isMovingX;
        return true;
    }

    private void SpawnTile()
    {
        lastTilePos = theStack[stackIndex].transform.localPosition;
        stackIndex--;

        if (stackIndex < 0)
            stackIndex = transform.childCount - 1;

        desiredPos = Vector3.down * scoreCount;

        theStack[stackIndex].transform.localPosition = new Vector3(0f, scoreCount, 0f);
        theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
    }

    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);
    }
}