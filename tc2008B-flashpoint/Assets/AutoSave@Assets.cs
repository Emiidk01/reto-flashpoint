using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoSaveEditor
{
    static AutoSaveEditor()
    {
        // Llama al método AutoSave cada minuto (60 segundos).
        EditorApplication.update += AutoSave;
    }

    private static void AutoSave()
    {
        // Verifica si ha pasado un minuto desde el último guardado.
        if (EditorApplication.timeSinceStartup % 30 < 1)
        {
            // Guarda la escena actual.
            if (EditorSceneManager.SaveOpenScenes())
            {
                Debug.Log("Escenas guardadas automáticamente.");
            }
            else
            {
                Debug.LogWarning("No se pudo guardar la escena automáticamente.");
            }
        }
    }
}
