// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Nalix.Logging.Extensions;

namespace Ascendance.Rendering.Scenes;

/// <summary>
/// The SceneManager class is responsible for managing scenes and objects within those scenes.
/// It handles scene transitions, object spawning, and object destruction.
/// </summary>
public static class SceneManager
{
    #region Events

    /// <summary>
    /// This event is invoked at the beginning of the next frame after all non-persisting objects have been queued to be destroyed
    /// and after the new objects have been queued to spawn, but before they are initialized.
    /// </summary>
    public static event System.Action<System.String, System.String> SceneChanged;

    #endregion Events

    #region Fields

    private static BaseScene _currentScene;
    private static System.String _nextScene = "";

    private static readonly System.Collections.Generic.List<BaseScene> _loadedScenes = [];
    private static readonly System.Collections.Generic.HashSet<SceneObject> _activeSceneObjects = [];
    private static readonly System.Collections.Generic.HashSet<SceneObject> _pendingSpawnObjects = [];
    private static readonly System.Collections.Generic.HashSet<SceneObject> _pendingDestroyObjects = [];

    #endregion Fields

    /// <summary>
    /// Retrieves all objects in the scene of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve.</typeparam>
    /// <returns>ScreenSize HashSet of all objects of the specified type.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.IReadOnlyCollection<T> GetAllObjectsOfType<T>()
        where T : SceneObject
        => System.Linq.Enumerable.ToList(System.Linq.Enumerable.OfType<T>(_activeSceneObjects));

    /// <summary>
    /// Queues a scene to be loaded on the next frame.
    /// </summary>
    /// <param name="name">The name of the scene to be loaded.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void RequestSceneChange(System.String name) => _nextScene = name;

    /// <summary>
    /// Queues a single object to be spawned in the scene.
    /// </summary>
    /// <param name="o">The object to be spawned.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void EnqueueSpawn(SceneObject o)
    {
        if (o.IsInitialized)
        {
            throw new System.Exception($"Instance of SceneObject {nameof(o)} already exists in Scenes");
        }
        if (!_pendingSpawnObjects.Add(o))
        {
            $"Instance of SceneObject {nameof(o)} is already queued to be spawned.".Warn();
        }
    }

    /// <summary>
    /// Queues a collection of objects to be spawned in the scene.
    /// </summary>
    /// <param name="sceneObjects">The collection of objects to be spawned.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void EnqueueSpawn(System.Collections.Generic.IEnumerable<SceneObject> sceneObjects)
    {
        foreach (SceneObject o in sceneObjects)
        {
            EnqueueSpawn(o);
        }
    }

    /// <summary>
    /// Queues an object to be destroyed in the scene.
    /// </summary>
    /// <param name="o">The object to be destroyed.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void EnqueueDestroy(SceneObject o)
    {
        if (!_activeSceneObjects.Contains(o) && !_pendingSpawnObjects.Contains(o))
        {
            throw new System.Exception("Instance of SceneObject does not exist in the scene.");
        }
        if (!_pendingSpawnObjects.Remove(o) && !_pendingDestroyObjects.Add(o))
        {
            "Instance of SceneObject is already queued to be destroyed.".Warn();
        }
    }

    /// <summary>
    /// Queues a collection of objects to be destroyed in the scene.
    /// </summary>
    /// <param name="sceneObjects">The collection of objects to be destroyed.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void EnqueueDestroy(System.Collections.Generic.IEnumerable<SceneObject> sceneObjects)
    {
        foreach (SceneObject o in sceneObjects)
        {
            EnqueueDestroy(o);
        }
    }

    /// <summary>
    /// Finds the first object of a specific type in the scene.
    /// </summary>
    /// <typeparam name="T">The type of object to find.</typeparam>
    /// <returns>The first object of the specified type, or null if none exist.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static T FindFirstObjectOfType<T>() where T : SceneObject
    {
        System.Collections.Generic.IReadOnlyCollection<T> objects = GetAllObjectsOfType<T>();
        return objects.Count != 0 ? System.Linq.Enumerable.First(objects) : null;
    }

    #region Internal Methods

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Boolean InSpawnQueue(this SceneObject o) => _pendingSpawnObjects.Contains(o);

    [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Boolean InDestroyQueue(this SceneObject o) => _pendingDestroyObjects.Contains(o);


    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static void ProcessSceneChange()
    {
        if (_nextScene == _currentScene?.Name) { _nextScene = ""; return; }

        if (_nextScene?.Length == 0)
        {
            return;
        }

        ClearScene();
        System.String lastScene = _currentScene?.Name ?? "";
        LoadScene(_nextScene);
        SceneChanged?.Invoke(lastScene, _nextScene);
        _nextScene = "";
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static void ProcessPendingDestroy()
    {
        foreach (SceneObject o in _pendingDestroyObjects)
        {
            if (!_activeSceneObjects.Remove(o))
            {
                "Instance of SceneObject to be destroyed does not exist in scene".Warn();
                continue;
            }
            o.OnBeforeDestroy();
        }
        _pendingDestroyObjects.Clear();
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static void ProcessPendingSpawn()
    {
        foreach (SceneObject q in _pendingSpawnObjects)
        {
            if (!_activeSceneObjects.Add(q))
            {
                throw new System.Exception("Instance of queued SceneObject already exists in scene.");
            }
        }

        _pendingSpawnObjects.Clear();

        foreach (SceneObject o in _activeSceneObjects)
        {
            if (!o.IsInitialized)
            {
                o.InternalInitialize();
            }
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static void UpdateSceneObjects(System.Single deltaTime)
    {
        foreach (SceneObject o in _activeSceneObjects)
        {
            if (o.IsEnabled)
            {
                o.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Creates instances of all classes inheriting from Scenes in the specified namespace.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
        "require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming",
        "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. " +
        "The return value of the source method does not have matching annotations.", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality",
        "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static void InitializeScenes()
    {
        // Get the types from the entry assembly that match the scene namespace
        System.Collections.Generic.IEnumerable<System.Type> sceneTypes =
            System.Linq.Enumerable.Where(
                System.Reflection.Assembly.GetEntryAssembly()!.GetTypes(), t => t.Namespace?.Contains(GraphicsEngine.GraphicsConfig.SceneNamespace) == true);

        // HashSet to check for duplicate scene names efficiently
        System.Collections.Generic.HashSet<System.String> sceneNames = [];

        foreach (System.Type type in sceneTypes)
        {
            // Skip compiler-generated types (like anonymous types or internal generic types)
            if (type.Name.Contains('<'))
            {
                continue;
            }

            // Check if the class has the IgnoredLoadAttribute
            if (System.Reflection.CustomAttributeExtensions.GetCustomAttribute<IgnoredLoadAttribute>(type) != null)
            {
                NLogixFx.Debug(
                    message: $"Skipping load of scene {type.Name} because it is marked as not loadable.",
                    source: type.Name);

                continue;
            }

            // Attempt to find a constructor with no parameters
            System.Reflection.ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
            if (constructor == null)
            {
                continue;
            }

            // Instantiate the scene using the parameterless constructor
            BaseScene scene;
            try
            {
                scene = (BaseScene)constructor.Invoke(null);
            }
            catch (System.Exception ex)
            {
                // Handle any exceptions that occur during instantiation
                ex.Error(source: type.Name, message: $"Error instantiating scene {type.Name}: {ex.Message}");
                continue;
            }

            // Check for duplicate scene names
            if (sceneNames.Contains(scene.Name))
            {
                NLogixFx.Error(message: $"Duplicate scene name '{scene.Name}' detected.", source: type.Name);
                throw new System.Exception($"Scenes with name {scene.Name} already exists.");
            }

            // Add the scene name to the HashSet for future checks
            _ = sceneNames.Add(scene.Name);

            // Add the scene to the list
            _loadedScenes.Add(scene);
        }

        // Switch to the main scene defined in the config
        RequestSceneChange(GraphicsEngine.GraphicsConfig.MainScene);
    }

    #endregion Internal Methods

    #region Private Methods

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void ClearScene()
    {
        foreach (SceneObject sceneObject in _activeSceneObjects)
        {
            if (!sceneObject.IsPersistent)
            {
                sceneObject.OnBeforeDestroy();
            }
        }
        _ = _activeSceneObjects.RemoveWhere(o => !o.IsPersistent);

        foreach (SceneObject queued in _pendingSpawnObjects)
        {
            if (!queued.IsPersistent)
            {
                queued.OnBeforeDestroy();
            }
        }
        _ = _pendingSpawnObjects.RemoveWhere(o => !o.IsPersistent);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void LoadScene(System.String name)
    {
        BaseScene found = System.Linq.Enumerable.FirstOrDefault(_loadedScenes, scene => scene.Name == name);
        if (found == null)
        {
            NLogixFx.Error(message: $"Scene '{name}' not found in scene list.", source: "SceneManager");
            throw new System.Exception($"Scene with name '{name}' does not exist.");
        }

        _currentScene = found;
        _currentScene.InitializeScene();
        EnqueueSpawn(_currentScene.GetObjects());
    }

    #endregion Private Methods
}