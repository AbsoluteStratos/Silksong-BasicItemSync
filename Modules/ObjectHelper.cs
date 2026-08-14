using System;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules;

internal static class ObjectHelper
{
    public static PersistentItem<T>? FindPersistent<T>(string sceneName, string id) where T : IEquatable<T>
    {
        var scene = SceneManager.GetActiveScene();
        return Resources.FindObjectsOfTypeAll<PersistentItem<T>>()
            .FirstOrDefault(p => p.ItemData.ID == id && p.ItemData.SceneName == sceneName && p.gameObject.scene == scene);
    }

    extension(Language)
    {
        public static string GetLocal(LocalisedString str)
        {
            return Language.Get(str.Key, str.Sheet);
        }
    }
}