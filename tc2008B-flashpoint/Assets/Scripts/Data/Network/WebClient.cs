// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Sergio Ruiz-Loza, Ph.D. March 2021

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class WebClient : MonoBehaviour
{

    public GameObject CeldaPrefab;
    public GameObject PuntoInteresPrefab;
    public GameObject FuegoPrefab;
    public GameObject PuertaPrefab;
    public GameObject EntradaPrefab;


    [System.Serializable]
    public class Celda
    {
        public int[] celdas;
    }

    [System.Serializable]
    public class PuntoInteres
    {
        public int row;
        public int col;
        public string type;
    }

    [System.Serializable]
    public class Fuego
    {
        public int row;
        public int col;
    }

    [System.Serializable]
    public class Puerta
    {
        public int r1;
        public int c1;
        public int r2;
        public int c2;
    }

    [System.Serializable]
    public class Entrada
    {
        public int row;
        public int col;
    }

    [System.Serializable]
    public class ConfigData
    {
        public List<Celda> celdas;
        public List<PuntoInteres> puntos_interes;
        public List<Fuego> fuego;
        public List<Puerta> puertas;
        public List<Entrada> entradas;
    }


    void Start()
    {
        StartCoroutine(GetConfigData());
    }

    // IEnumerator - yield return
    IEnumerator GetConfigData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm("http://localhost:8585", ""))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonReceived = webRequest.downloadHandler.text;
                Debug.Log("JSON recibido: " + jsonReceived); // Agregar este log
                ConfigData config = JsonUtility.FromJson<ConfigData>(webRequest.downloadHandler.text);

                // Aquí puedes instanciar los prefabs según los datos de config
                RenderizarMapa(config);
            }
        }
    }

    // Start is called before the first frame update
    void RenderizarMapa(ConfigData config)
    {
        // Instanciar celdas
        for (int i = 0; i < config.celdas.Count; i++)
        {
            Debug.Log("config.celdas[i].celdas.Length");
            //for (int j = 0; j < config.celdas[i].celdas.Length; j++)
            //{
            // Instancia la celda en la posición (i, j)
            //Vector3 position = new Vector3(i, 0, j); // Posición en Unity, ajusta según sea necesario
            // Instantiate(CeldaPrefab, position, Quaternion.identity);
            //Debug.Log($"Celda instanciada en posición: {position}"); // Log para verificar la posición

            // Aquí puedes configurar las paredes de la celda basadas en los dígitos de config.celdas[i][j]
            //}
        }

        // Instanciar puntos de interés
        foreach (var punto in config.puntos_interes)
        {
            Vector3 position = new Vector3(punto.row, 0, punto.col); // Posición en Unity
            GameObject prefab = PuntoInteresPrefab;
            if (punto.type == "v") // Si es víctima
            {
                // Cambia prefab si tienes uno específico para víctimas
            }
            else if (punto.type == "f") // Si es falsa alarma
            {
                // Cambia prefab si tienes uno específico para falsas alarmas
            }
            Instantiate(prefab, position, Quaternion.identity);
        }

        // Instanciar fuego
        foreach (var fuego in config.fuego)
        {
            Vector3 position = new Vector3(fuego.row, 0, fuego.col); // Posición en Unity
            Instantiate(FuegoPrefab, position, Quaternion.identity);
            Debug.Log($"Fuego instanciado en posición: {position}"); // Log para fuego
        }

        // Instanciar puertas
        foreach (var puerta in config.puertas)
        {
            Vector3 position1 = new Vector3(puerta.r1, 0, puerta.c1); // Posición en Unity para el primer extremo de la puerta
            Vector3 position2 = new Vector3(puerta.r2, 0, puerta.c2); // Posición en Unity para el segundo extremo de la puerta
            Instantiate(PuertaPrefab, (position1 + position2) / 2, Quaternion.identity); // Posicionar la puerta en el centro entre las dos celdas
            Debug.Log($"Puerta instanciada entre posición: {position1} y {position2}"); // Log para puertas


            // Puedes rotar la puerta o ajustarla según cómo se conecten las celdas
        }

        // Instanciar entradas
        foreach (var entrada in config.entradas)
        {
            Vector3 position = new Vector3(entrada.row, 0, entrada.col); // Posición en Unity
            Instantiate(EntradaPrefab, position, Quaternion.identity);
            Debug.Log($"Entrada instanciada en posición: {position}"); // Log para entradas
        }
    }
}
