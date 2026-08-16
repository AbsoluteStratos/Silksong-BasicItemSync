using BasicItemSync.Modules.Network.Server;
using GenericVariableExtension;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BasicItemSync.Modules.Network.Client
{
    internal class SettingsUI : MonoBehaviour
    {
        public bool ShowSettings = false;
        const int width = 500;
        const int x = (1920 - width) / 2;

        Rect WindowRect = new(x, 20, width, 360);
        Vector2 ScrollPosition = Vector2.zero;

        public SyncServerSettings Settings;
        public Dictionary<string, bool> SettingValues = [];

        public void SetProps()
        {
            foreach (var prop in SyncServerSettings.GetProperties())
            {
                SettingValues[prop.Name] = Settings.GetVariable<bool>(prop.Name);
                Log.LogDebug(prop.Name, SettingValues[prop.Name]);
            }
        }

        void OnGUI()
        {
            if (ShowSettings)
            {
                var color = GUI.backgroundColor;
                GUI.backgroundColor = Color.black;

                WindowRect = GUI.Window(987, WindowRect, DrawUI, "BasicItemSync Settings");

                GUI.backgroundColor = color;
            }
        }

        void DrawUI(int id)
        {

            // Settings scroll section
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Width(500), GUILayout.Height(300));

            foreach (var setting in SettingValues.ToList())
            {
                SettingValues[setting.Key] = GUILayout.Toggle(setting.Value, setting.Key);
            }

            GUILayout.EndScrollView();

            // Divider between settings and buttons
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));

            // Buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save"))
            {
                ShowSettings = false;
                NetworkSender.SendSettings(SettingValues);
                Destroy(this);
                return;
            }

            if (GUILayout.Button("Cancel"))
            {
                ShowSettings = false;
                Destroy(this);
                return;
            }


            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 1920, 1080));
        }
    }
}
