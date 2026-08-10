using System;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules;

internal class ObjectHelper
{
    public static PersistentItem<T>? FindPersistent<T>(string sceneName, string id) where T : IEquatable<T>
    {
        var currentScene = SceneManager.GetActiveScene();
        var objs = currentScene.GetRootGameObjects();

        foreach (var obj in objs)
        {
            var components = obj.GetComponentsInChildren<PersistentItem<T>>();
            foreach (var item in components)
            {
                if (item.ItemData.ID == id && item.ItemData.SceneName == sceneName)
                {
                    return item;
                }
            }
        }

        return null;
    }
}