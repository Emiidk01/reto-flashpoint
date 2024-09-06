# Reto Flash Point

<video controls src="VisualizaciónGrafica3D_Reto.mp4" title="Title"></video>

#### Archivos
Flashpoint_ModelServer.py es nustro Modelo y Servidor estable
SimulacionModel.py es nuestra simulacion en con mesa mejorada (solo que esta version no se conecta al cliente)
WebClient.cs es nuestro cliente que se encuentra en:
tc2008B-flashpoint > Assets > Scripts > Data > Network > WebClient.cs

## Reto
-------------------------------------------------------------------------------------------------------------------------
### Trasfondo

Las actividades de búsqueda y rescate de víctimas en catástrofes a gran escala son problemas sociales de gran relevancia y, desde un punto de vista científico, plantean numerosos problemas técnicos diferentes en los campos de la inteligencia artificial, la robótica y los sistemas multiagente. Por esta razón, es importante el desarrollo de herramientas que permitan explorar estrategias adecuadas para este tipo de actividades.

### Descripción

El reto de este bloque fue implementar una simulación basada en agentes del juego de mesa “Flash Point: Fire Rescue” de Indie Boards & Cards. Por esta razón, las reglas de esta simulación están basadas en las reglas del juego de mesa.

### Escenario
El espacio de la simulación es un espacio bidimensional no toroidal de 6 x 8 celdas cuya descripción se recibe a través de un archivo de texto.
El archivo empieza con 6 líneas de 8 grupos de 4 dígitos. Cada grupo de 4 dígitos representa una celda del espacio; cada dígito representa las paredes de una celda (arriba, izquierda, abajo, derecha). El número 1 representa que si tiene pared, el 0 que no tiene. Por ejemplo, 1010 indica que la celda sólo tiene pared arriba y abajo.
A continuación, los marcadores de puntos de interés, 3 líneas de tres elementos. El primer elemento, un número entero, representa el renglón. El segundo elemento, un número entero, representa la columna. Y el tercer elemento, un carácter, indicando si es una víctima (v) o una falsa alarma (f).
Enseguida, los marcadores de fuego, 10 líneas de 2 elementos. El primer elemento, un número entero, representa el renglón. El segundo elemento, un número entero, representa la columna. 
Posteriormente, los marcadores de puerta, 8 líneas con 4 números enteros, r1, c1, r2, c2, indicando las dos celdas que conecta una puerta en particular.
Por último, los puntos de entrada, 4 líneas de 2 números enteros, indicando el renglón y la columna de los puntos de entrada.



El ejemplo que se muestra a continuación muestra la configuración inicial de la Figura 1.
1001 1000 1100 1001 1100 1001 1000 1100
0001 0000 0110 0011 0110 0011 0010 0110
1000 0100 1001 1000 1000 1100 1001 1100
0011 0110 0011 0010 0010 0110 0011 0110
1001 1000 1000 1000 1100 1001 1100 1101
0011 0010 0010 0010 0110 0011 0110 0111
2 4 v
5 1 f
5 8 v
2 2
2 3
3 2
3 3 
3 4
3 5
4 4
5 6
5 7
6 6
1 3 1 4
2 5 2 6
2 8 3 8
3 2 3 3 
4 4 5 4
4 6 4 7
8 5 8 6
8 7 8 8
1 6
3 1
4 8
6 3

### Objetivos
Desarrolla un modelo multiagente con una visualización en 3D que permita rescatar a las victimas atrapadas dentro de edificios en llamas antes de que el fuego quede fuera de control o que el edificio se desplome.
