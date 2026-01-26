using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Scenes;

namespace Ascendance.Rendering.Extensions;

internal static class SceneExtensions
{
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Boolean InSpawnQueue(this SceneObject o) => SceneManager.Instance.PendingSpawnObjects.Contains(o);

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Boolean InDestroyQueue(this SceneObject o) => SceneManager.Instance.PendingDestroyObjects.Contains(o);
}
