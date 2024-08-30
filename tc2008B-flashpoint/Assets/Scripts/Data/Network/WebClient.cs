// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Sergio Ruiz-Loza, Ph.D. March 2021

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class WebClient : MonoBehaviour
{
    public GameObject sphere;  // La esfera que quieres mover

    // IEnumerator - yield return
    IEnumerator SendData(string data)
    {
        WWWForm form = new WWWForm();
        form.AddField("bundle", "the data");
        string url = "http://localhost:8585";
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();          // Envía los datos al servidor
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Recibe la respuesta del servidor y mueve la esfera
                Debug.Log(www.downloadHandler.text);
                Vector3 newPosition = JsonUtility.FromJson<Vector3>(www.downloadHandler.text.Replace('\'', '\"'));
                sphere.transform.position = newPosition;
                Debug.Log("Esfera movida a: " + newPosition);
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 fakePos = new Vector3(3.44f, 0, -15.707f);  // Posición inicial de prueba
        string json = JsonUtility.ToJson(fakePos);
        StartCoroutine(SendData(json));
    }
}
