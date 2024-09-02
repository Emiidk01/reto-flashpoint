// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Sergio Ruiz-Loza, Ph.D. March 2021

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


public class WebClient : MonoBehaviour
{

    public GameObject WallPrefab;
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
        public List<Dictionary<string, string>> celdas { get; set; }

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
                Debug.Log("JSON recibido: " + jsonReceived);

                // Deserializar el JSON a un diccionario

                ConfigData configData = JsonConvert.DeserializeObject<ConfigData>(jsonReceived);
                // Acceder a los datos deserializados
                
                RenderizarMapa(configData);
            }
        }
    }

    // Start is called before the first frame update
    void RenderizarMapa(ConfigData config)
    {
        // Instanciar paredes (pendiente)
        if (config == null)
        {
            Debug.LogError("Config data is null!");
            return;
        }

        if (config.celdas == null)
        {
            Debug.LogError("Config data's celdas is null!");
            return;
        }
        List<Dictionary<string, string>> celdas = config.celdas;

        int row = 0;
        foreach (var fila in celdas)
        {
            row++;
            foreach (var kvp in fila)
            {
                Debug.Log("clave: " + kvp.Key);
                Debug.Log("valor: " + kvp.Value);
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    Debug.Log("caracter: " + kvp.Value[i]);
                }
                int col = int.Parse(kvp.Key); // Convertir la clave de string a int
                Vector3 position = new Vector3(row, 0, col);

                if (kvp.Value[0] == '1') // Pared arriba
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        
                        Vector3 wallPosition = position + new Vector3(-0.5f, 0, 0);
                        Instantiate(WallPrefab, wallPosition, Quaternion.Euler(0, 90, 0));
                    }
                    if (kvp.Value[1] == '1') // Pared izquierda
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        
                        Vector3 wallPosition = position + new Vector3(0, 0, -0.5f);
                        Instantiate(WallPrefab, wallPosition, Quaternion.identity);
                    }
                    if (kvp.Value[2] == '1') // Pared abajo
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        Vector3 wallPosition = position + new Vector3(0.5f, 0, 0);
                        Instantiate(WallPrefab, wallPosition, Quaternion.Euler(0, 90, 0));
                    }
                    if (kvp.Value[3] == '1') // Pared derecha
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        
                        Vector3 wallPosition = position + new Vector3(0, 0, 0.5f);
                        Instantiate(WallPrefab, wallPosition, Quaternion.identity);
                    }
            
                    
                }
            }

        // Instanciar puntos de interés (funciona)
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
            Instantiate(prefab, position, prefab.transform.rotation);
        }

        // Instanciar fuego (funciona)
        foreach (var fuego in config.fuego)
        {
            Vector3 position = new Vector3(fuego.row, 0, fuego.col); // Posición en Unity
            Instantiate(FuegoPrefab, position, Quaternion.identity);
            Debug.Log($"Fuego instanciado en posición: {position}"); // Log para fuego
        }

        // Instanciar puertas (funciona)
        foreach (var puerta in config.puertas)
        {
            Vector3 position1 = new Vector3(puerta.r1, 0, puerta.c1); // Posición de la primera celda
            Vector3 position2 = new Vector3(puerta.r2, 0, puerta.c2); // Posición de la segunda celda
            Vector3 position = (position1 + position2) / 2; // Posición central para la puerta

            Quaternion rotation;

            // Determinar si la puerta es horizontal o vertical
            if (puerta.c1 == puerta.c2)
            {
                // Las celdas están en la misma fila, la puerta es vertical
                rotation = Quaternion.Euler(0, 90, 0); // Rotación de 90 grados en Y
            }
            else
            {
                // Las celdas están en la misma columna, la puerta es horizontal
                rotation = Quaternion.identity; // No rotación (o 0 grados en Y)
            }

            // Instanciar la puerta en la posición y con la rotación calculada
            Instantiate(PuertaPrefab, position, rotation);
            Debug.Log($"Puerta instanciada en posición: {position} con rotación: {rotation.eulerAngles}");
        }

        // Instanciar entradas (funciona)
        foreach (var entrada in config.entradas)
        {
            Vector3 position = new Vector3(entrada.row, 0, entrada.col); // Posición en Unity
            Quaternion rotation = Quaternion.identity; // No rotación por defecto

            if (entrada.row == 1) // Si la entrada está en la fila superior
            {
                rotation = Quaternion.Euler(0, 90, 0); // Apunta hacia abajo
                Vector3 position2 = position + new Vector3(-0.5f, 0, 0);
                Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada

            }
            else if (entrada.row == 6) // Si la entrada está en la fila inferior
            {
                rotation = Quaternion.Euler(0, 90, 0); // Apunta hacia arriba
                Vector3 position2 = position + new Vector3(0.5f, 0, 0);
                Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada
            }
            else if (entrada.col == 1) // Si la entrada está en la primera columna
            {
                rotation = Quaternion.Euler(0, 0, 0); // Apunta hacia la derecha
                Vector3 position2 = position + new Vector3(0, 0, -0.5f);
                Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada
            }
            else if (entrada.col == 8) // Si la entrada está en la última columna
            {
                rotation = Quaternion.Euler(0, 0, 0); // Apunta hacia la izquierda
                Vector3 position2 = position + new Vector3(0, 0, 0.5f);
                Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada

            }

            // Debug.Log($"Entrada instanciada en posición: {position} con rotación: {rotation.eulerAngles}"); // Log para entradas
        }
    }
}
