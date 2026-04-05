using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }

    private Queue<FloatingTextRequest> requestQueue = new Queue<FloatingTextRequest>();
    private bool isProcessingQueue = false;

    [System.Serializable]
    public class Settings
    {
        public GameObject PLAYER;

        public int poolSize = 20;

        [Header("Colors")]
        public Color expColor = new Color(1f, 0.5f, 0f);
        public Color eventColor = new Color(1f, 0.5f, 0f);
        public Color resourceColor = new Color(1f, 0.5f, 0f);
        public Color crystalColor = new Color(1f, 0.5f, 0f);
        public Color keyColor = new Color(1f, 0.5f, 0f);

        [Header("Animation")]
        public float duration = 1.2f;
        public float delayBetweenSpawns = 0.3f;
        public Vector3 offset = new Vector3(0, 2f, 0);

        public float startScale = 1f;
        public float endScale = 1.2f;

        public AnimationCurve alphaCurve;
        public AnimationCurve motionCurve;
        public AnimationCurve scaleCurve;


    }

    [Header("Setup")]
    public FloatingText prefab;
    public Settings settings;

    private Queue<FloatingText> pool = new Queue<FloatingText>();
    private Dictionary<FloatingTextType, Color> colorMap;

    [Header("Singleton")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //if (dontDestroyOnLoad)
        //    DontDestroyOnLoad(gameObject);

        InitColors();
        InitPool();
    }

    void InitPool()
    {
        for (int i = 0; i < settings.poolSize; i++)
        {
            var obj = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    void InitColors()
    {
        colorMap = new Dictionary<FloatingTextType, Color>()
        {
            { FloatingTextType.Resource,  settings.resourceColor },
            { FloatingTextType.Experience, settings.expColor},
            { FloatingTextType.Crystal, settings.crystalColor},
            { FloatingTextType.Key, settings.keyColor },
            { FloatingTextType.Event, settings.eventColor  }
        };
    }

    FloatingText Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        // Expansión dinámica opcional
        var obj = Instantiate(prefab, transform);
        return obj;
    }

    void ReturnToPool(FloatingText text)
    {
        text.gameObject.SetActive(false);
        pool.Enqueue(text);
    }

    public void SpawnText(string content, FloatingTextType type)
    {
        if (settings.PLAYER == null)
        {
            Debug.LogError("[SpawnText] Player reference is missing in FloatingTextPool settings.");
            return;
        }
        requestQueue.Enqueue(new FloatingTextRequest(content, type));

        if (!isProcessingQueue)
            StartCoroutine(ProcessQueue());
    }

    IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;

        while (requestQueue.Count > 0)
        {
            var request = requestQueue.Dequeue();

            ShowText(request);

            yield return new WaitForSeconds(settings.delayBetweenSpawns);
        }

        isProcessingQueue = false;
    }

    void ShowText(FloatingTextRequest request)
    {
        var target = settings.PLAYER.transform;

        var text = Get();

        // Posición sin heredar rotación
        text.transform.position = target.position;
        text.transform.rotation = text.transform.rotation;

        text.gameObject.SetActive(true);
        Color color = colorMap[request.type];
        text.Play(request.content, color, settings);

        StartCoroutine(ReturnAfterTime(text));
    }

    IEnumerator ReturnAfterTime(FloatingText text)
    {
        yield return new WaitForSeconds(settings.duration);
        ReturnToPool(text);
    }
}
public enum FloatingTextType
{
    Resource,
    Experience,
    Crystal,
    Key,
    Event
}

class FloatingTextRequest
{
    public string content;
    public FloatingTextType type;

    public FloatingTextRequest(string content, FloatingTextType type)
    {
        this.content = content;
        this.type = type;
    }
}