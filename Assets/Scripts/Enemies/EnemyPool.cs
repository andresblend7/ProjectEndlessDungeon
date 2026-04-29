using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool 
{
    public string lol;
    // ─── Estado interno ───────────────────────────────────────────────────────

    private readonly GameObject _prefab;
    private readonly Transform _parent;           // padre de jerarquía para mantener la escena ordenada
    private readonly bool _allowExpansion;
    private readonly Queue<GameObject> _available; // instancias listas para usar
    private readonly HashSet<GameObject> _active;  // instancias actualmente en el mundo

    // ─── Propiedades públicas ─────────────────────────────────────────────────

    /// <summary>Cantidad de enemigos de esta pool que están activos en la escena.</summary>
    public int ActiveCount => _active.Count;

    // ─── Constructor ──────────────────────────────────────────────────────────

    /// <param name="prefab">Prefab del enemigo a instanciar.</param>
    /// <param name="initialSize">Instancias pre-creadas al inicializar la pool.</param>
    /// <param name="parent">Transform padre para mantener la jerarquía limpia.</param>
    /// <param name="allowExpansion">Si es true, se pueden crear nuevas instancias cuando la cola está vacía.</param>
    public EnemyPool(GameObject prefab, int initialSize, Transform parent, bool allowExpansion)
    {
        _prefab = prefab;
        _parent = parent;
        _allowExpansion = allowExpansion;
        _available = new Queue<GameObject>(initialSize);
        _active = new HashSet<GameObject>();

        // Pre-instanciación síncrona; usa WarmUpAsync para pools grandes
        for (int i = 0; i < initialSize; i++)
            _available.Enqueue(CreateInstance());
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve una instancia lista para usar.
    /// Si la cola está vacía y la expansión está permitida, crea una nueva instancia.
    /// Retorna null si no hay instancias disponibles y no se permite expansión.
    /// </summary>
    public GameObject Get()
    {
        GameObject obj;

        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else if (_allowExpansion)
        {
            // Expansión dinámica: se crea una instancia adicional bajo demanda
            obj = CreateInstance();
        }
        else
        {
            return null; // pool agotada y sin permiso de expansión
        }

        // Activar y notificar al componente pooleable
        obj.SetActive(true);
        _active.Add(obj);
        SubscribeToDeactivation(obj);

        if (obj.TryGetComponent<IPooledObject>(out var pooled))
            pooled.OnSpawn();

        return obj;
    }

    /// <summary>
    /// Devuelve un objeto al pool: lo desactiva y lo encola para reutilización.
    /// Llamado internamente cuando el objeto dispara su evento OnDeactivate.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        if (!_active.Contains(obj))
            return; // ya fue devuelto o pertenece a otra pool

        UnsubscribeFromDeactivation(obj);
        _active.Remove(obj);
        obj.SetActive(false);
        obj.transform.SetParent(_parent);
        _available.Enqueue(obj);
    }

    /// <summary>
    /// Pre-calienta el pool de forma asíncrona para no congelar el frame inicial.
    /// Instancia <paramref name="extraCount"/> objetos adicionales repartidos en varios frames.
    /// </summary>
    public IEnumerator WarmUpAsync(int extraCount, int instancesPerFrame = 2)
    {
        int spawned = 0;
        while (spawned < extraCount)
        {
            int batch = Mathf.Min(instancesPerFrame, extraCount - spawned);
            for (int i = 0; i < batch; i++)
                _available.Enqueue(CreateInstance());

            spawned += batch;
            yield return null; // espera un frame entre lotes
        }
    }

    /// <summary>
    /// Destruye todas las instancias. Llamar solo al cerrar la escena o al hacer limpieza final.
    /// </summary>
    public void Dispose()
    {
        foreach (var obj in _available)
            if (obj != null) UnityEngine.Object.Destroy(obj);

        foreach (var obj in _active)
            if (obj != null) UnityEngine.Object.Destroy(obj);

        _available.Clear();
        _active.Clear();
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

    private GameObject CreateInstance()
    {
        // Se instancia desactivado para que Awake no se dispare hasta que sea entregado
        var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
        obj.SetActive(false);
        return obj;
    }

    private void SubscribeToDeactivation(GameObject obj)
    {
        if (obj.TryGetComponent<IPooledObject>(out var pooled))
            pooled.OnDeactivate += ReturnToPool;
    }

    private void UnsubscribeFromDeactivation(GameObject obj)
    {
        if (obj.TryGetComponent<IPooledObject>(out var pooled))
            pooled.OnDeactivate -= ReturnToPool;
    }
}

/// <summary>
/// Interfaz que debe implementar todo enemigo que sea gestionado por un ObjectPool.
/// Permite al pool suscribirse al ciclo de vida del objeto sin acoplarse a su tipo concreto.
/// </summary>
public interface IPooledObject
{
    /// <summary>
    /// Se dispara cuando el objeto quiere ser devuelto a la pool (al morir, al salir de rango, etc.).
    /// El argumento es el propio GameObject para que el pool pueda identificarlo.
    /// </summary>
    event Action<GameObject> OnDeactivate;

    /// <summary>
    /// Llamado por el pool justo antes de entregar el objeto al mundo.
    /// Úsalo para reiniciar vida, animaciones, estado interno, etc.
    /// </summary>
    void OnSpawn();
}

