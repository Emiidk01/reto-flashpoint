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
    public GameObject CrackedWallPrefab;
    public GameObject BrokenWallPrefab;


    public GameObject PuntoInteresPrefab;
    public GameObject FuegoPrefab;
    public GameObject PuertaPrefab;
    public GameObject EntradaPrefab;


    private float nextUpdateTime = 0f; // Time control for updates
    private float updateInterval = 1f; // Update every 1 second


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

    void Update()
    {
        // Control the update frequency
        if (Time.time >= nextUpdateTime)
        {
            // Simulate receiving JSON data
            string simulatedJson = SimulateJsonData();
            ApplyJsonChanges(simulatedJson);

            nextUpdateTime = Time.time + updateInterval;
        }
    }

    string SimulateJsonData()
    {
        // Simulated JSON string
        return "{ \"walls\": { \"2,3 Right\": \"break\", \"3,7 Top\": \"crack\" }, \"doors\": { \"1,3 1,4\": \"open\", \"2,8 3,8\": \"destroy\" }, \"POIS\": { \"5 8\": \"reveal\" } }";
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
                        GameObject wall = Instantiate(WallPrefab, wallPosition, Quaternion.Euler(0, 90, 0));
                        wall.name = $"({row},{col}) Top Wall";
                    }
                    if (kvp.Value[1] == '1') // Pared izquierda
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        
                        Vector3 wallPosition = position + new Vector3(0, 0, -0.5f);
                        GameObject wall = Instantiate(WallPrefab, wallPosition, Quaternion.identity);
                        wall.name = $"({row},{col}) Left Wall";

                    }
                    if (kvp.Value[2] == '1') // Pared abajo
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        Vector3 wallPosition = position + new Vector3(0.5f, 0, 0);
                        GameObject wall = Instantiate(WallPrefab, wallPosition, Quaternion.Euler(0, 90, 0));
                        wall.name = $"({row},{col}) Bottom Wall";
                        
                    }
                    if (kvp.Value[3] == '1') // Pared derecha
                    {
                        if (WallPrefab == null)
                        {
                            Debug.LogError("WallPrefab is not assigned!");
                            continue;
                        }
                        
                        Vector3 wallPosition = position + new Vector3(0, 0, 0.5f);
                        GameObject wall =Instantiate(WallPrefab, wallPosition, Quaternion.identity);
                        wall.name = $"({row},{col}) Right Wall";
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
            GameObject puntoInteres = Instantiate(prefab, position, prefab.transform.rotation);
            puntoInteres.name = $"({punto.row},{punto.col}) Point of Interest ({punto.type})";

        }

        // Instanciar fuego (funciona)
        foreach (var fuego in config.fuego)
        {
            Vector3 position = new Vector3(fuego.row, 0, fuego.col); // Posición en Unity
            GameObject fuegoObjeto = Instantiate(FuegoPrefab, position, Quaternion.identity);
            fuegoObjeto.name = $"({fuego.row},{fuego.col}) Fire";
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
                string objectName = $"({puerta.r1},{puerta.c1}) Bottom Wall"; // Nombre del objeto que buscas
                GameObject posibleWall = GameObject.Find(objectName);

                if (posibleWall != null)
                {
                    Destroy(posibleWall);
                }
                string objectName2 = $"({puerta.r2},{puerta.c2}) Top Wall"; // Nombre del objeto que buscas
                GameObject posibleWall2 = GameObject.Find(objectName2);

                if (posibleWall2 != null)
                {
                    Destroy(posibleWall2);
                }
            }
            else
            {
                // Las celdas están en la misma columna, la puerta es horizontal
                rotation = Quaternion.identity; // No rotación (o 0 grados en Y)
                string objectName = $"({puerta.r1},{puerta.c1}) Right Wall"; // Nombre del objeto que buscas
                GameObject posibleWall = GameObject.Find(objectName);

                if (posibleWall != null)
                {
                    Destroy(posibleWall);
                }
                string objectName2 = $"({puerta.r2},{puerta.c2}) Left Wall"; // Nombre del objeto que buscas
                GameObject posibleWall2 = GameObject.Find(objectName2);

                if (posibleWall2 != null)
                {
                    Destroy(posibleWall2);
                }
            }
            

            // Instanciar la puerta en la posición y con la rotación calculada
            GameObject puertaObjeto =Instantiate(PuertaPrefab, position, rotation);
            puertaObjeto.name = $"({puerta.r1},{puerta.c1}) to ({puerta.r2},{puerta.c2}) Door";

            Debug.Log($"Puerta instanciada en posición: {position} con rotación: {rotation.eulerAngles}");
        }

        // Instanciar entradas (funciona)
        foreach (var entrada in config.entradas)
        {
            Vector3 position = new Vector3(entrada.row, 0, entrada.col); // Posición en Unity
            Quaternion rotation = Quaternion.identity; // No rotación por defecto

            if (entrada.row == 1) // Si la entrada está en la fila superior
            {   
                string objectName = $"({entrada.row},{entrada.col}) Top Wall"; // Nombre del objeto que buscas
                GameObject posibleWall = GameObject.Find(objectName);

                if (posibleWall != null)
                {
                    Destroy(posibleWall);
                }

                rotation = Quaternion.Euler(0, 90, 0); // Apunta hacia abajo
                Vector3 position2 = position + new Vector3(-0.5f, 0, 0);
                GameObject entradaObjeto = Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada
                entradaObjeto.name = $"({entrada.row},{entrada.col}) Entry";
                

            }
            else if (entrada.row == 6) // Si la entrada está en la fila inferior
            {
                string objectName = $"({entrada.row},{entrada.col}) Bottom Wall"; // Nombre del objeto que buscas
                GameObject posibleWall = GameObject.Find(objectName);

                if (posibleWall != null)
                {
                    Destroy(posibleWall);
                }
                
                rotation = Quaternion.Euler(0, 90, 0); // Apunta hacia arriba
                Vector3 position2 = position + new Vector3(0.5f, 0, 0);
                GameObject entradaObjeto = Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada
                entradaObjeto.name = $"({entrada.row},{entrada.col}) Entry";

            }
            else if (entrada.col == 1) // Si la entrada está en la primera columna
            {
                string objectName = $"({entrada.row},{entrada.col}) Left Wall"; // Nombre del objeto que buscas
                GameObject posibleWall = GameObject.Find(objectName);

                if (posibleWall != null)
                {
                    Destroy(posibleWall);
                }
                rotation = Quaternion.Euler(0, 0, 0); // Apunta hacia la derecha
                Vector3 position2 = position + new Vector3(0, 0, -0.5f);
                GameObject entradaObjeto = Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada
                entradaObjeto.name = $"({entrada.row},{entrada.col}) Entry";
            }
            else if (entrada.col == 8) // Si la entrada está en la última columna
            {
                string objectName = $"({entrada.row},{entrada.col}) Right Wall"; // Nombre del objeto que buscas
                GameObject posibleWall = GameObject.Find(objectName);

                if (posibleWall != null)
                {
                    Destroy(posibleWall);
                }
                rotation = Quaternion.Euler(0, 0, 0); // Apunta hacia la izquierda
                Vector3 position2 = position + new Vector3(0, 0, 0.5f);
                GameObject entradaObjeto = Instantiate(EntradaPrefab, position2, rotation); // Usar la rotación calculada
                entradaObjeto.name = $"({entrada.row},{entrada.col}) Entry";

            }
        }

        
    }

    void ApplyJsonChanges(string json)
    {
        // Parse the JSON
        Dictionary<string, Dictionary<string, string>> changes = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        Debug.Log("Actualizando mapa...");
        // Process walls
        if (changes.ContainsKey("walls"))
        {
            foreach (var wallChange in changes["walls"])
            {
                string[] wallKey = wallChange.Key.Split(' ');
                string location = wallKey[0]; // e.g., "0,0"
                string wallType = wallKey[1]; // e.g., "Left"
                string wallType2 = "";
                string newLocation = "";

                if (wallType == "Top"){
                    wallType2 = "Bottom"; 
                    // Split the string to get the row and column
                    string[] coordinates = location.Split(',');

                    // Convert to an integer and add 1
                    int row = int.Parse(coordinates[0]) - 1;
                    int col = int.Parse(coordinates[1]);

                    // Reassemble the new coordinates back into a string
                    newLocation = $"{row},{col}"; // Creates the string in the "row,col" format
                }
                else if (wallType == "Bottom"){
                    wallType2 = "Top"; 
                    string[] coordinates = location.Split(',');

                    int row = int.Parse(coordinates[0]) + 1;
                    int col = int.Parse(coordinates[1]);

                    newLocation = $"{row},{col}"; 
                }
                else if (wallType == "Left"){
                    wallType2 = "Right";
                    string[] coordinates = location.Split(',');

                    int row = int.Parse(coordinates[0]);
                    int col = int.Parse(coordinates[1]) - 1;

                    newLocation = $"{row},{col}"; 
                }
                else if (wallType == "Right"){
                    wallType2 = "Left";
                    string[] coordinates = location.Split(',');

                    int row = int.Parse(coordinates[0]);
                    int col = int.Parse(coordinates[1]) + 1;

                    newLocation = $"{row},{col}"; 
                }

                string action = wallChange.Value; // e.g., "break"

                // Find the wall GameObject by name
                string wallName = $"({location}) {wallType} Wall";
                string wallName2 = $"({newLocation}) {wallType2} Wall";

                GameObject wallObject = GameObject.Find(wallName);
                GameObject wallObject2 = GameObject.Find(wallName2);


                if (wallObject != null)
                {
                    // Apply changes based on action
                    if (action == "break")
                    {   
                        GameObject brokenWall = Instantiate(BrokenWallPrefab, wallObject.transform.position, wallObject.transform.rotation);
                        brokenWall.name = wallName;
                        // Destroy the wall
                        Destroy(wallObject);
                    }
                    else if (action == "crack")
                    {
                        GameObject crackedWall = Instantiate(CrackedWallPrefab, wallObject.transform.position, wallObject.transform.rotation);
                        crackedWall.name = wallName;
                        // Apply visual effects or change state
                        Destroy(wallObject);
                    }
                }
                if (wallObject2 != null)
                {
                    // Apply changes based on action
                    if (action == "break")
                    {   
                        GameObject brokenWall = Instantiate(BrokenWallPrefab, wallObject2.transform.position, wallObject2.transform.rotation);
                        brokenWall.name = wallName2;
                        // Destroy the wall
                        Destroy(wallObject2);
                    }
                    else if (action == "crack")
                    {
                        GameObject crackedWall = Instantiate(CrackedWallPrefab, wallObject2.transform.position, wallObject2.transform.rotation);
                        crackedWall.name = wallName2;
                        // Apply visual effects or change state
                        Destroy(wallObject2);
                    }
                }
            }
        }

        // Process doors
        if (changes.ContainsKey("doors"))
        {
            foreach (var doorChange in changes["doors"])
            {
                string[] doorKey = doorChange.Key.Split(' ');
                string doorLocation1 = doorKey[0]; // e.g., "0,3"
                string doorLocation2 = doorKey[1]; // e.g., "1,3"
                string action = doorChange.Value; // e.g., "open"

                string doorName = $"({doorLocation1}) to ({doorLocation2}) Door";
                GameObject doorObject = GameObject.Find(doorName);

                if (doorObject != null)
                {
                    // find doors in wall
                    Transform backHarrow = doorObject.transform.Find("BackHarrow");
                    Transform frontHarrow = doorObject.transform.Find("FrontHarrow");
                    if (backHarrow != null && frontHarrow != null)
                    {
                        if (action == "open")
                        {
                            // Apply logic for opening the doors
                            backHarrow.localPosition = new Vector3(backHarrow.localPosition.x, 0, backHarrow.localPosition.z);
                            backHarrow.localScale = new Vector3(backHarrow.localScale.x, 1, backHarrow.localScale.z);

                            frontHarrow.localPosition = new Vector3(frontHarrow.localPosition.x, 0, frontHarrow.localPosition.z);
                            frontHarrow.localScale = new Vector3(frontHarrow.localScale.x, 1, frontHarrow.localScale.z);
                        }
                        else if (action == "close")
                        {
                            // Apply logic for closing the door
                            backHarrow.localPosition = new Vector3(backHarrow.localPosition.x, -0.6f , backHarrow.localPosition.z);
                            backHarrow.localScale = new Vector3(backHarrow.localScale.x, 1.5f , backHarrow.localScale.z);

                            frontHarrow.localPosition = new Vector3(frontHarrow.localPosition.x, -0.6f , frontHarrow.localPosition.z);
                            frontHarrow.localScale = new Vector3(frontHarrow.localScale.x, 1.5f , frontHarrow.localScale.z);
                        }
                        else if (action == "destroy")
                        {
                            GameObject brokenDoor = Instantiate(BrokenWallPrefab, doorObject.transform.position, doorObject.transform.rotation);
                            brokenDoor.name = doorName;
                            // Destroy the door
                            Destroy(doorObject);
                        }
                    }
                }
            }
        }

        // // Process points of interest
        // if (changes.ContainsKey("POIS"))
        // {
        //     foreach (var poiChange in changes["POIS"])
        //     {
        //         string location = poiChange.Key; // e.g., "0 1"
        //         string action = poiChange.Value; // e.g., "reveal"

        //         string poiName = $"({location.Replace(" ", ",")}) Point of Interest";
        //         GameObject poiObject = GameObject.Find(poiName);

        //         if (poiObject != null)
        //         {
        //             if (action == "reveal")
        //             {
        //                 // Apply reveal logic
        //                 poiObject.SetActive(true);
        //             }
        //         }
        //     }
        // }
    }
}
