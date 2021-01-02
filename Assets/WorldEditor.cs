using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor
{

  World world;
  Editor noiseEditor;

  public override void OnInspectorGUI()
  {
    using (var check = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI();
      if (check.changed)
      {
        world.CreateWorld();
      }
    }

    DrawSettingsEditor(world.noiseSettingsEditor, ref world.noiseSettingsFoldout, ref noiseEditor);
  }

  void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
  {
    if (settings != null)
    {
      foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
      using (var check = new EditorGUI.ChangeCheckScope())
      {
        if (foldout)
        {
          CreateCachedEditor(settings, null, ref editor);
          editor.OnInspectorGUI();

          if (check.changed)
          {
            world.CreateWorld();
          }
        }
      }
    }
  }

  private void OnEnable()
  {
    world = (World)target;
  }
}
