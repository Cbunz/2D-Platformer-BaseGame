using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PhysicsHelper : MonoBehaviour {

    static PhysicsHelper instance;
    static PhysicsHelper Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<PhysicsHelper>();

            if (instance != null)
            {
                return instance;
            }

            Create();

            return instance;
        }
        set
        {
            instance = value;
        }
    }

    static void Create()
    {
        GameObject physicsHelperGameObject = new GameObject("PhysicsHelper");
        instance = physicsHelperGameObject.AddComponent<PhysicsHelper>();
    }

    Dictionary<Collider2D, MovingPlatform> movingPlatformCache = new Dictionary<Collider2D, MovingPlatform>();
    Dictionary<Collider2D, PlatformEffector2D> platformEffectorCache = new Dictionary<Collider2D, PlatformEffector2D>();
    Dictionary<Collider2D, Tilemap> tilemapCache = new Dictionary<Collider2D, Tilemap>();

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        PopulateColliderDictionary(movingPlatformCache);
        PopulateColliderDictionary(platformEffectorCache);
        PopulateColliderDictionary(tilemapCache);
    }

    protected void PopulateColliderDictionary<TComponent>(Dictionary<Collider2D, TComponent> dict)
        where TComponent : Component
    {
        TComponent[] components = FindObjectsOfType<TComponent>();

        for (int i = 0; i < components.Length; i++)
        {
            Collider2D[] componentColliders = components[i].GetComponents<Collider2D>();

            for (int j = 0; j < componentColliders.Length; j++)
            {
                dict.Add(componentColliders[j], components[i]);
            }
        }
    }

    public static bool ColliderHasMovingPlatform(Collider2D collider)
    {
        return Instance.movingPlatformCache.ContainsKey(collider);
    }
    public static bool ColliderHasPlatformEffector(Collider2D collider)
    {
        return Instance.platformEffectorCache.ContainsKey(collider);
    }
    public static bool ColliderHasTilemap(Collider2D collider)
    {
        return Instance.tilemapCache.ContainsKey(collider);
    }
    public static bool TryGetMovingPlatform(Collider2D collider, out MovingPlatform movingPlatform)
    {
        return Instance.movingPlatformCache.TryGetValue(collider, out movingPlatform);
    }
    public static bool TryGetPlatformEffector(Collider2D collider, out PlatformEffector2D platformEffector)
    {
        return Instance.platformEffectorCache.TryGetValue(collider, out platformEffector);
    }
    public static bool TryGetTilemap(Collider2D collider, out Tilemap tilemap)
    {
        return Instance.tilemapCache.TryGetValue(collider, out tilemap);
    }
}
