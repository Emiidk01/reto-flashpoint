// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Sergio Ruiz-Loza, Ph.D. March 2021

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class WebClient : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject firePrefab;
    public GameObject poiPrefab;
    public GameObject doorPrefab;
    public GameObject entryPrefab;

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
            //www.SetRequestHeader("Content-Type", "text/html");
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();          // Envia la solicitud al servidor de python
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Procesa la respuesta recibida por el servidor
                Debug.Log(www.downloadHandler.text);    // ARespuesta de python
                JObject mapData = JObject.Parse(www.downloadHandler.text);
                RenderMap(mapData);
                //Debug.Log("Form upload complete!");
            }
        }
    }

    // Método para renderizar el mapa basado en los datos recibidos
    void RenderMap(JObject mapData)
    {
        // Aquí deberías implementar la lógica para instanciar los objetos en el mapa
        // Utiliza los datos de 'mapData' para posicionar paredes, fuegos, POIs, puertas, etc.
        Debug.Log("Map data received and parsed.");
    }


    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(SendData(call));
        StartCoroutine(SendData(json));
        // transform.localPosition
    }

    // Update is called once per frame
    void Update()
    {
        // Puedes agregar aquí lógica que necesite actualizaciones por frame, si es necesario
    }
}
