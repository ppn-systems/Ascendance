// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;

namespace Ascendance.Rendering.Scenes;

/// <summary>
/// Represents a sealed class that stores information about a scene change and persists across scene transitions.
/// </summary>
/// <typeparam name="T">The type of information stored in the scene change info.</typeparam>
public sealed class SceneTransitionData<T> : SceneObject
{
    #region Fields

    private System.Boolean _hasSceneChanged;
    private readonly T _data;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the name associated with this scene change info.
    /// </summary>
    public System.String Name { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneTransitionData{T}"/> class with the specified information and name.
    /// </summary>
    /// <param name="info">The information to store.</param>
    /// <param name="name">The name associated with this scene change info.</param>
    public SceneTransitionData(T info, System.String name)
    {
        _data = info;
        Name = name;
        base.IsPersistent = true;
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Extracts the stored information.
    /// </summary>
    /// <returns>The information of type <typeparamref name="T"/>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
         System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public T GetData() => _data;

    /// <summary>
    /// Initializes the scene change info and subscribes to the SceneChanged event.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override void Initialize() => SceneManager.Instance.SceneChanged += this.OnSceneChange;

    /// <summary>
    /// Cleans up before the object is destroyed and unsubscribes from the SceneChanged event.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void OnBeforeDestroy() => SceneManager.Instance.SceneChanged -= this.OnSceneChange;

    /// <summary>
    /// Updates the state of the SceneChangeInfo object and destroys it after a scene change.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void Update(System.Single deltaTime)
    {
        // Destroy this instance on the first frame after a new scene has been loaded
        if (_hasSceneChanged)
        {
            base.Destroy();
        }
    }

    /// <summary>
    /// Retrieves a SceneChangeInfo object by name, or returns a default value if not found.
    /// </summary>
    /// <param name="name">The name of the scene change info to look for.</param>
    /// <param name="defaultValue">The default value to return if no matching object is found.</param>
    /// <returns>The stored information of type <typeparamref name="T"/> or the default value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static T FindByName(System.String name, T defaultValue)
    {
        SceneTransitionData<T> info = SceneManager.Instance.GetFirstActive<SceneTransitionData<T>>();
        return info == null ? defaultValue : info.Name != name ? defaultValue : info.GetData();
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Handles the scene change event by setting the <see cref="_hasSceneChanged"/> flag.
    /// </summary>
    /// <param name="lastScene">The name of the last scene.</param>
    /// <param name="nextScene">The name of the next scene.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void OnSceneChange(System.String lastScene, System.String nextScene) => _hasSceneChanged = true;

    #endregion Private Methods
}
