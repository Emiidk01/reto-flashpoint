using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json; // Asegúrate de tener la referencia al paquete Newtonsoft.Json

public class WebClient : MonoBehaviour
{
    public GameObject EMPTY; // id.value = 0
    public GameObject WallPrefab; // id.value = 9
    public GameObject PuntoInteresPrefab; // id.value = 2
    public GameObject FuegoPrefab; // id.value = 4
    public GameObject PuertaPrefab; // id.value = 10
    public GameObject EntradaPrefab; // id.value = 8
    public GameObject BomberosPrefab; // id.value = 12

    [System.Serializable]
    public class PuntoInteres
    {
        public int row;
        public int col;
        public string type;
    }

    [System.Serializable]
    public class ConfigData
    {
        public List<PuntoInteres> puntos_interes;
    }

    // Lista para mantener referencias a los objetos instanciados
    private Dictionary<(int, int), GameObject> instantiatedObjects = new Dictionary<(int, int), GameObject>();

    void Start()
    {
        // Inicia la coroutine para la actualización periódica de los datos
        StartCoroutine(UpdateDataPeriodically());
    }

    IEnumerator UpdateDataPeriodically()
    {
        while (true)
        {
            // Obtiene los datos de configuración y actualiza el mapa
            yield return StartCoroutine(GetConfigData());
            yield return new WaitForSeconds(0.7f); // Espera 0.7 segundos antes de la siguiente solicitud
        }
    }

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
                Debug.Log("JSON recibido: " + jsonReceived);

                // Deserializar el JSON a ConfigData
                ConfigData configData = JsonConvert.DeserializeObject<ConfigData>(jsonReceived);
                UpdateMap(configData);
            }
        }
    }

    void UpdateMap(ConfigData config)
    {
        if (config == null || config.puntos_interes == null)
        {
            Debug.LogError("Config data or puntos_interes is null!");
            return;
        }

        // Crear un nuevo mapa temporal
        var newObjects = new Dictionary<(int, int), GameObject>();

        foreach (var punto in config.puntos_interes)
        {
            Vector3 position = new Vector3(punto.row, 0, punto.col); // Posición en Unity

            GameObject prefab = null;
            switch (punto.type)
            {
                case "2":
                    prefab = PuntoInteresPrefab;
                    break;
                case "4":
                    prefab = FuegoPrefab;
                    break;
                case "8":
                    prefab = EntradaPrefab;
                    break;
                case "9":
                    prefab = WallPrefab;
                    break;
                case "10":
                    prefab = PuertaPrefab;
                    break;
                case "12":
                    prefab = BomberosPrefab;
                    break;
                default:
                    Debug.LogWarning("Unknown prefab type: " + punto.type);
                    continue;
            }

            if (prefab != null)
            {
                GameObject instantiatedObject;

                if (instantiatedObjects.TryGetValue((punto.row, punto.col), out instantiatedObject))
                {
                    // Actualizar la posición del objeto existente
                    instantiatedObject.transform.position = position;
                    // Actualizar el prefab si es necesario
                    if (instantiatedObject.name != prefab.name)
                    {
                        Destroy(instantiatedObject);
                        instantiatedObject = Instantiate(prefab, position, Quaternion.identity);
                    }
                }
                else
                {
                    // Instanciar un nuevo objeto
                    instantiatedObject = Instantiate(prefab, position, Quaternion.identity);
                }

                // Añadir a la lista de objetos instanciados
                newObjects[(punto.row, punto.col)] = instantiatedObject;
            }
        }

        // Destruir los objetos que ya no están en el mapa
        foreach (var obj in instantiatedObjects)
        {
            if (!newObjects.ContainsKey(obj.Key))
            {
                Destroy(obj.Value);
            }
        }

        // Actualizar la lista de objetos instanciados
        instantiatedObjects = newObjects;
    }
}
