using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RingEnemySpawner : MonoBehaviour
{
    // ─── Parámetros de spawn ──────────────────────────────────────────────────

    [Header("Spawn Ring Settings")]

    [Tooltip("Radio mínimo de spawn. Los enemigos nunca aparecerán más cerca que este valor.")]
    [SerializeField] private float minSpawnRadius = 5f;

    [Tooltip("Radio máximo de spawn. Los enemigos nunca aparecerán más lejos que este valor.")]
    [SerializeField] private float maxSpawnRadius = 10f;

    [Tooltip("Offset vertical de la posición de spawn respecto al jugador.")]
    [SerializeField] private float spawnHeight = 0f;

    [Tooltip("Intervalo base en segundos entre ciclos de spawn.")]
    [SerializeField] private float baseSpawnRate = 3f;

    [Tooltip("Factor de reducción del intervalo con la dificultad. " +
             "nuevoIntervalo = base / (1 + dificultad * factor).")]
    [SerializeField] private float rateReductionFactor = 0.1f;

    [Tooltip("Intentos de spawn por ciclo. Cada intento puede generar varios enemigos según la PoolConfig.")]
    [SerializeField] private int spawnsPerWaveBase = 1;

    // ─── Límite global de enemigos ────────────────────────────────────────────

    [Header("Global Enemy Cap")]

    [Tooltip("Si está activo, limita el total de enemigos activos en la escena.")]
    [SerializeField] private bool useGlobalEnemyCap = false;

    [Tooltip("Límite global base de enemigos activos (solo si useGlobalEnemyCap = true).")]
    [SerializeField] private int globalMaxEnemiesBase = 30;

    [Tooltip("Factor de escala del límite global con la dificultad.")]
    [SerializeField] private float globalMaxDifficultyFactor = 1f;

    // ─── Detección de obstáculos en el punto de spawn ─────────────────────────

    [Header("Spawn Obstacle Check")]

    [Tooltip("Si está activo, verifica que no haya geometría en el punto de spawn.")]
    [SerializeField] private bool checkObstacles = true;

    [Tooltip("Radio de la esfera usada para comprobar obstáculos.")]
    [SerializeField] private float obstacleCheckRadius = 0.5f;

    [Tooltip("Capas consideradas como obstáculo para el spawn.")]
    [SerializeField] private LayerMask obstacleLayerMask;

    [Tooltip("Número máximo de reintentos de posición si hay un obstáculo.")]
    [SerializeField] private int maxPositionRetries = 5;

    // ─── Referencias ──────────────────────────────────────────────────────────

    [Header("References")]

    [Tooltip("Transform del jugador. Si se deja vacío se usa el transform padre de este objeto.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("RunManager de la partida. Si se deja vacío se busca automáticamente en Awake.")]
    [SerializeField] private RoomTimerController runManager;

    // ─── Configuración de pools ───────────────────────────────────────────────

    [Header("Enemy Pools")]

    [Tooltip("Lista de configuraciones de pools de enemigos.")]
    [SerializeField] private List<PoolConfig> pools = new List<PoolConfig>();

    // ─── Estado interno ───────────────────────────────────────────────────────

    private Coroutine _spawnLoopCoroutine;
    private float _currentDifficulty;
    private float _maxDifficulty = 10f;

    // Lista de pools disponibles para el ciclo actual (se regenera al cambiar dificultad)
    private readonly List<PoolConfig> _activePools = new List<PoolConfig>();
    private float _lastDifficultyForActivePoolsRefresh = -1f;

    // Padre jerárquico para las instancias del pool (mantiene la escena ordenada)
    private Transform _poolParent;

    // ─── Ciclo de vida Unity ──────────────────────────────────────────────────

    private void Awake()
    {
        var poolGO = new GameObject("[EnemyPool]");
        _poolParent = poolGO.transform;

        // Resolver la referencia al jugador
        if (playerTransform == null)
            playerTransform = transform.parent != null ? transform.parent : transform;

        if (runManager == null)
            runManager = FindFirstObjectByType<RoomTimerController>();

        if (runManager == null)
        {
            Debug.LogError("[RingEnemySpawner] No se encontró un RoomTimerController en la escena.", this);
            return;
        }

        runManager.OnDifficultyChanged.AddListener(HandleDifficultyChanged);
        _maxDifficulty = runManager.maxDificultfactor;

        // Inicializar pools en Awake para que estén listas antes de cualquier Start
        InitializePools();
    }

    private void Start()
    {
        if (runManager == null) return;

        if (runManager.isStarted)
            StartSpawnLoop();
    }

    private void OnDestroy()
    {
        if (runManager != null)
            runManager.OnDifficultyChanged.RemoveListener(HandleDifficultyChanged);

        foreach (var config in pools)
            config.Pool?.Dispose();
    }

    // ─── Inicialización de pools ──────────────────────────────────────────────

    private void InitializePools()
    {
        if (pools == null || pools.Count == 0)
        {
            Debug.LogWarning("[RingEnemySpawner] La lista 'pools' está vacía.", this);
            return;
        }

        for (int i = 0; i < pools.Count; i++)
        {
            var config = pools[i];
            if (config == null) continue;

            if (config.enemyPrefab == null)
            {
                Debug.LogError($"[RingEnemySpawner] pools[{i}] no tiene 'enemyPrefab' asignado.", this);
                continue;
            }

            config.Pool = new EnemyPool(
                config.enemyPrefab,
                config.initialPoolSize,
                _poolParent,
                config.allowPoolExpansion
            );
        }
    }

    // ─── Manejadores de eventos del RunManager ────────────────────────────────

    private void HandleRunStarted() => StartSpawnLoop();
    private void HandleRunPaused() => StopSpawnLoop();
    private void HandleRunResumed() => StartSpawnLoop();
    private void HandleRunEnded() => StopSpawnLoop();

    private void HandleDifficultyChanged(float newDifficulty)
    {
        _currentDifficulty = newDifficulty;
        // Forzar recálculo de pools activas en el próximo ciclo
        _lastDifficultyForActivePoolsRefresh = -1f;
    }

    // ─── Control de la corutina ───────────────────────────────────────────────

    private void StartSpawnLoop()
    {
        if (_spawnLoopCoroutine != null)
            StopCoroutine(_spawnLoopCoroutine);

        _spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    private void StopSpawnLoop()
    {
        if (_spawnLoopCoroutine != null)
        {
            StopCoroutine(_spawnLoopCoroutine);
            _spawnLoopCoroutine = null;
        }
    }

    // ─── Corutina principal de spawn ──────────────────────────────────────────

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float currentRate = baseSpawnRate / (1f + _currentDifficulty * rateReductionFactor);
            currentRate = Mathf.Max(currentRate, 0.1f);

            yield return new WaitForSeconds(currentRate);

            RefreshActivePoolsIfNeeded();

            if (_activePools.Count == 0)
                continue;

            for (int wave = 0; wave < spawnsPerWaveBase; wave++)
            {
                PoolConfig selectedPool = SelectPoolWeighted();
                if (selectedPool == null)
                    break;

                float normalizedDiff = Mathf.Clamp01(_currentDifficulty / _maxDifficulty);
                int countToSpawn = selectedPool.GetSpawnCount(normalizedDiff);
                int maxActive = selectedPool.GetMaxActive(_currentDifficulty);

                for (int i = 0; i < countToSpawn; i++)
                {
                    if (selectedPool.Pool.ActiveCount >= maxActive)
                        break;

                    // Límite global: suma real de activos en todas las pools, sin contador manual
                    if (useGlobalEnemyCap && GetTotalActiveEnemies() >= GetGlobalMax())
                        goto EndWave;

                    Vector3 spawnPos;
                    if (!TryGetSpawnPosition(out spawnPos))
                        continue;

                    GameObject enemy = selectedPool.Pool.Get();
                    if (enemy == null)
                        break;

                    enemy.transform.position = spawnPos;
                }
            }

        EndWave:;
        }
    }

    // ─── Helpers de spawn ─────────────────────────────────────────────────────

    /// <summary>
    /// Genera una posición aleatoria en el anillo entre minSpawnRadius y maxSpawnRadius,
    /// siempre centrada en la posición ACTUAL del jugador en el momento del spawn.
    /// Como se lee playerTransform.position en tiempo real (no cacheado),
    /// el anillo sigue al jugador aunque se mueva entre ciclos de spawn.
    /// </summary>
    private bool TryGetSpawnPosition(out Vector3 position)
    {
        // Leer posición del jugador en tiempo real para que el anillo lo siga
        Vector3 playerPos = playerTransform.position;

        for (int attempt = 0; attempt <= maxPositionRetries; attempt++)
        {
            // Dirección aleatoria sobre el plano XZ
            Vector2 dir2D = UnityEngine.Random.insideUnitCircle.normalized;

            // Radio aleatorio dentro del anillo [minSpawnRadius, maxSpawnRadius]
            float radius = UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius);

            position = playerPos
                       + new Vector3(dir2D.x, 0f, dir2D.y) * radius
                       + Vector3.up * spawnHeight;

            if (!checkObstacles)
                return true;

            if (!Physics.CheckSphere(position, obstacleCheckRadius, obstacleLayerMask))
                return true;
        }

        position = Vector3.zero;
        return false;
    }

    private PoolConfig SelectPoolWeighted()
    {
        if (_activePools.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var p in _activePools)
            totalWeight += p.selectionWeight;

        if (totalWeight <= 0f) return _activePools[0];

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var p in _activePools)
        {
            cumulative += p.selectionWeight;
            if (roll <= cumulative)
                return p;
        }

        return _activePools[_activePools.Count - 1];
    }

    private void RefreshActivePoolsIfNeeded()
    {
        if (Mathf.Approximately(_currentDifficulty, _lastDifficultyForActivePoolsRefresh))
            return;

        _activePools.Clear();
        foreach (var p in pools)
        {
            if (p.Pool == null) continue;
            if (p.IsAvailableAt(_currentDifficulty))
                _activePools.Add(p);
        }

        _lastDifficultyForActivePoolsRefresh = _currentDifficulty;
    }

    /// <summary>
    /// Suma los enemigos activos de todas las pools.
    /// Fuente de verdad para el límite global; no requiere contadores manuales.
    /// </summary>
    private int GetTotalActiveEnemies()
    {
        int total = 0;
        foreach (var p in pools)
            if (p.Pool != null) total += p.Pool.ActiveCount;
        return total;
    }

    private int GetGlobalMax()
    {
        return globalMaxEnemiesBase + Mathf.RoundToInt(_currentDifficulty * globalMaxDifficultyFactor);
    }

    // ─── Gizmos de editor ─────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawSpawnRingGizmo();
    }

    private void DrawSpawnRingGizmo()
    {
        // El gizmo se centra en el jugador si está asignado, si no en este transform
        Vector3 center = (playerTransform != null ? playerTransform.position : transform.position)
                         + Vector3.up * spawnHeight;

        // Zona segura interior (radio mínimo) — azul
        Handles.color = new Color(0.2f, 0.6f, 1f, 0.15f);
        Handles.DrawSolidDisc(center, Vector3.up, minSpawnRadius);
        Handles.color = new Color(0.2f, 0.6f, 1f, 0.8f);
        Handles.DrawWireDisc(center, Vector3.up, minSpawnRadius);

        // Zona de spawn (entre min y max) — naranja
        Handles.color = new Color(1f, 0.4f, 0f, 0.12f);
        Handles.DrawSolidDisc(center, Vector3.up, maxSpawnRadius);
        Handles.color = new Color(1f, 0.4f, 0f, 0.9f);
        Handles.DrawWireDisc(center, Vector3.up, maxSpawnRadius);

        // Líneas discontinuas radiales solo en la zona de spawn (min → max)
        int radialLines = 12;
        for (int i = 0; i < radialLines; i++)
        {
            float angle = i * (360f / radialLines) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            float dashLength = 0.4f;
            float gap = 0.2f;
            float traveled = minSpawnRadius; // empezar desde el borde interior
            bool drawing = true;

            while (traveled < maxSpawnRadius)
            {
                float segLen = drawing ? dashLength : gap;
                float end = Mathf.Min(traveled + segLen, maxSpawnRadius);

                if (drawing)
                    Gizmos.DrawLine(center + dir * traveled, center + dir * end);

                traveled += segLen;
                drawing = !drawing;
            }
        }

        // Etiquetas de radio
        Handles.Label(center + Vector3.right * minSpawnRadius,
                       $"  min={minSpawnRadius:F1}", EditorStyles.miniLabel);
        Handles.Label(center + Vector3.right * maxSpawnRadius,
                       $"  max={maxSpawnRadius:F1}  h={spawnHeight:F1}", EditorStyles.miniLabel);

        DrawPoolDifficultyGizmos(center);
    }

    private void DrawPoolDifficultyGizmos(Vector3 center)
    {
        if (pools == null) return;

        float angleStep = pools.Count > 0 ? 360f / pools.Count : 0f;
        for (int i = 0; i < pools.Count; i++)
        {
            var p = pools[i];
            if (p == null || !p.enabled) continue;

            Color poolColor = Color.HSVToRGB((float)i / Mathf.Max(pools.Count, 1), 0.8f, 1f);
            poolColor.a = 0.8f;
            Handles.color = poolColor;

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 tipPos = center + dir * (maxSpawnRadius + 1.5f);

            Handles.DrawLine(center + dir * (maxSpawnRadius + 0.1f), tipPos);

            string label = p.enemyPrefab != null ? p.enemyPrefab.name : "null";
            string diffRange = p.maxDifficulty > 0f
                ? $"[{p.minDifficulty:F0}–{p.maxDifficulty:F0}]"
                : $"[{p.minDifficulty:F0}+]";

            Handles.Label(tipPos, $"  {label}\n  diff {diffRange}", EditorStyles.miniLabel);
        }
    }
#endif
}


[Serializable]
public class PoolConfig
{
    [Tooltip("Prefab del enemigo que se instanciará en esta pool.")]
    public GameObject enemyPrefab;

    [Tooltip("Número de instancias pre-creadas al inicio de la partida.")]
    public int initialPoolSize = 10;

    [Tooltip("La pool no participa si la dificultad actual es menor que este valor.")]
    public float minDifficulty = 0f;

    [Tooltip("La pool no participa si la dificultad actual es mayor. Valor ≤ 0 = sin límite superior.")]
    public float maxDifficulty = 0f;

    [Tooltip("Cuántos enemigos de esta pool se intentan spawnear en cada activación.")]
    public int baseCountPerSpawn = 1;

    [Tooltip("Multiplicador sobre baseCountPerSpawn en función de la dificultad normalizada (0-1). " +
             "Resultado: Mathf.RoundToInt(baseCount * curva.Evaluate(diffNorm)).")]
    public AnimationCurve countByDifficultyCurve = AnimationCurve.Linear(0, 1, 1, 2);

    [Tooltip("Límite base de enemigos activos simultáneos de esta categoría.")]
    public int maxActiveBase = 5;

    [Tooltip("Incremento del límite de activos por punto de dificultad. " +
             "maxActivos = maxActiveBase + Round(difficulty * factor).")]
    public float difficultyMaxActiveFactor = 0.5f;

    [Tooltip("Desactiva este pool sin borrar su configuración.")]
    public bool enabled = true;

    [Tooltip("Peso para la selección aleatoria ponderada entre pools disponibles. " +
             "Mayor peso = mayor probabilidad de ser elegida.")]
    public float selectionWeight = 1f;

    [Tooltip("Si es true, se pueden crear instancias adicionales cuando la pool se vacía.")]
    public bool allowPoolExpansion = true;

    [NonSerialized] public EnemyPool Pool;

    public int GetMaxActive(float currentDifficulty)
    {
        int dynamic = maxActiveBase + Mathf.RoundToInt(currentDifficulty * difficultyMaxActiveFactor);
        return Mathf.Max(dynamic, maxActiveBase);
    }

    public int GetSpawnCount(float normalizedDifficulty)
    {
        float multiplier = countByDifficultyCurve.Evaluate(normalizedDifficulty);
        return Mathf.Max(1, Mathf.RoundToInt(baseCountPerSpawn * multiplier));
    }

    public bool IsAvailableAt(float difficulty)
    {
        if (!enabled) return false;
        if (difficulty < minDifficulty) return false;
        if (maxDifficulty > 0f && difficulty > maxDifficulty) return false;
        return true;
    }
}