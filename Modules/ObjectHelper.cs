using System;
using TeamCherry.Localization;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules;

internal static class ObjectHelper
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

    extension(Language)
    {
        public static string Get(LocalisedString str)
        {
            return Language.Get(str.Key, str.Sheet);
        }
    }
}