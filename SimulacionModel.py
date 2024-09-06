from mesa import Agent, Model
from mesa.space import MultiGrid
from mesa.time import RandomActivation
from mesa.datacollection import DataCollector
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import numpy as np
import time
import datetime
import random
import matplotlib
plt.rcParams["animation.html"] = "jshtml"
matplotlib.rcParams['animation.embed_limit'] = 2**128
import heapq
import time
import json
from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
from sys import argv
import threading
import requests 

def astar_pathfinding(grid_state, start, goal):
    def heuristic(a, b):
        # Manhattan distance heuristic
        return abs(a[0] - b[0]) + abs(a[1] - b[1])

    def get_neighbors(node):
        (x, y) = node
        # Possible moves (right, left, down, up)
        neighbors = [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]
        valid_neighbors = []
        for (nx, ny) in neighbors:
            # Check if neighbor is within bounds and not a wall (value 10)
            if 0 <= nx < len(grid_state) and 0 <= ny < len(grid_state[0]) and grid_state[nx][ny] != 9:
                valid_neighbors.append((nx, ny))
        return valid_neighbors

    open_set = []
    heapq.heappush(open_set, (0 + heuristic(start, goal), 0, start))
    came_from = {}
    cost_so_far = {}
    came_from[start] = None
    cost_so_far[start] = 0

    while open_set:
        _, current_cost, current = heapq.heappop(open_set)

        if current == goal:
            # Reconstruct path
            path = []
            while current:
                path.append(current)
                current = came_from[current]
            return path[::-1]

        for neighbor in get_neighbors(current):
            new_cost = cost_so_far[current] + 1  # Assume each step has the same cost
            if neighbor not in cost_so_far or new_cost < cost_so_far[neighbor]:
                cost_so_far[neighbor] = new_cost
                priority = new_cost + heuristic(goal, neighbor)
                heapq.heappush(open_set, (priority, new_cost, neighbor))
                came_from[neighbor] = current

    return None

class POI:
    def __init__(self, red_coor, black_coor, type):
        self.red_coor = red_coor
        self.black_coor = black_coor
        self.type = type


# CONFIGURACIÓN DEL JUEGO, obtenidos de setDeNumeros.txt
SPAWNEO_BOMBEROS = [(6,0), (8,0), (14, 6), (14, 4), (8, 18), (0, 12)] # generalist only
SPAWNEO_FIREMARKERS = []
SPAWNEO_PUERTAS = [] # 8 puertas
SPAWNEO_EXTERIORES = []
SPAWNEO_PAREDES = []
POOL_POIS = ['f', 'f', 'f', 'f', 'f', 'v', 'v', 'v', 'v', 'v', 'v', 'v', 'v', 'v', 'v'] # 10 Victimas
SPAWNEO_POIS = [] 
WC_VICTIMAS = 0
LC_VICTIMAS = 0
FILA_DE_BOMEROS = []
 
# MSIGUIENTE MISION: flame vs victims 
# While flame count > 5: DIJKSTRA(FLAMES) else: DIJKSTRA(POIS)
# get danger_weight DIJKSTRA thme based on danger_weight
# primer intento: Quita las flamas y con solo un bombero logra rescatar las victimas CON DIJKSTRA
# Dijkstra must have built-in con las walls en el pathfinding

# SIGUIENTE MISION: QUE LOS ROBOTS SE MUEVAN EN STEPS SEPARADOS Y NO EN UN STEP COMBINADO

# LAST MISION: FLAME PROMOTION SYSTEM
# if smoke spawns near a fire it gets instantly promoted to fire side
# if smoke spawns ON a fire it instantly explodes
# the dice pretty much adds smoke and thats it
# the Explosion() goes into another Explosion() if flame in cruz
# since it has Chain_Reaction() this stops at: moore=False
#   1. Wall nearby gets one damage point/ two damage points
#   2. Empty space gets turned into flame
#   3. Door gets destroyed and permanently gets openned resulting in:
#       3.1 If openned, then flame at empty in the same vector
#       3.2 If closed, only door gets destroyed
#   If flame spawns at POI and POI.hasVictim() then victim teleported 
# THE FLIPPING OF SMOKE IS ALSO IN Chain_reaction()

        # if total_flames < 6:
            # 1. self.path() towards POI
            # 2. if self.has_victim == True, self.path() towards exit
        # else: # meaning total_flames more than 5
            # 1. self.path() towards highest danger_level flame
            # 2. extinguish everything in the way in a radius of 1
        # self.path clears at the end of the turn and only moves in range of ACTION_POINTS

class BomberoAgent(Agent):
    def __init__(self, unique_id, model, spawn_point):
        super().__init__(unique_id, model)
        self.id = unique_id
        self.action_points = 0  
        self.path = []
        self.target_cell = ()
        self.has_victim = False
        self.poi_aux = '' # just here for simplicity
        self.action_per_turn = True
        self.spawn_point = spawn_point

    def step(self):
        global WC_VICTIMAS
        # Habilidad 0 - Atravesar puertas y pois no ocupables
        # espera, el poi que sea, si lo atraviesa se descubre siempre
        # debes tener un marcador para victimas reales
        if self.model.grid_state[self.pos] == 12:
            self.model.grid_state[self.pos] = 0

        # Habilidad 1 - Rescatar victima
        if self.has_victim == False:
            if self.pos == self.target_cell:
                    if self.poi_aux == 'v':
                        self.has_victim = True
                        #print("Point of interest at: " + str(self.pos) + ", which is: " + item.type)
                    #else:
                        #print("FASLE ALARMA YEYEYEYEY")
                    # si le poi existe en el agente, siempre redireccion
                    self.path.clear()
                    self.model.poi_refill()
                    self.model.grid_state[self.pos] = 0
        if (self.pos[0] == 0 or self.pos[1] == 0) and self.has_victim:
            #print("Se ha rescatado una victima!")
            WC_VICTIMAS += 1
            self.has_victim = False
            self.path.clear()

        # Habilidad 2 - Apagar fuegos
        red_coor, black_coor = self.pos
        cruz_r2 = [(red_coor, black_coor-2), (red_coor-2, black_coor), (red_coor, black_coor+2), (red_coor+2, black_coor)]
        for coor in cruz_r2:
            x,y = coor
            if x > 0 and y > 0 and x < HEIGTH and y < WIDTH:
                if self.model.grid_state[(coor)] == 7:
                    self.model.grid_state[(coor)] = 0
                    
        apagar_fuego = False
        cruz_r1 = list(self.model.grid.get_neighborhood(self.pos, moore=False, include_center=False))
        for coor_r1 in cruz_r1:
            cruz_r2 = list(self.model.grid.get_neighborhood(coor_r1, moore=False, include_center=False))
            for coor_r2 in cruz_r2:
                if self.model.grid_state[coor_r2] == 4:
                    self.model.grid_state[coor_r2] = 0
                    break  # Sale del bucle interno
            if apagar_fuego:
                break  # Sale del bucle externo

        # Habilidad CLAVE - Activation & Travel
        if self.path:  # If there is a path, keep moving
            next_position = self.path.pop(0)
            if (0 <= next_position[0] < self.model.grid.width and
                0 <= next_position[1] < self.model.grid.height):
                if self.model.grid.is_cell_empty(next_position):
                    self.model.grid.move_agent(self, next_position)
        elif self.has_victim:  # If there is a victim, find a path to the rescue location
            self.path_finder((6, 0)) # SET TO SPAWN POINT, SELF.SPAWN_POINT
        else:  # If there is no path and no victim, find a path to a POI
            if SPAWNEO_POIS:  # Check if SPAWNEO_POIS is not empty
                poi = SPAWNEO_POIS.pop()  # as a fila, appends at the end, works on the last one
                self.target_cell = (poi.red_coor, poi.black_coor)  # Define the coordinates
                self.poi_aux = poi.type
                self.path_finder(self.target_cell)

        # Activation & Travel
        if self.model.grid_state[self.pos] == 0:
            self.model.grid_state[self.pos] = 12

    def path_finder(self, target):
        start = self.pos
        if 0 <= target[0] < self.model.grid.width and 0 <= target[1] < self.model.grid.height:
            self.path = astar_pathfinding(self.model.grid_state, start, target)
            #print(f"Path found from {start} to {target}: {self.path}")
        #else:
            #print(f"Target {target} is out of bounds.")

def get_grid(model): # Función auxiliar para mejorar la grid
    return model.grid_state.copy()

class BomberoModel(Model):
    def __init__(self, HEIGTH, WIDTH):
        super().__init__()
        self.grid = MultiGrid(HEIGTH, WIDTH, torus=False)
        self.schedule = RandomActivation(self)
        self.grid_state = np.zeros((HEIGTH, WIDTH), dtype=int)  # Estado de la cuadrícula
        self.datacollector = DataCollector(model_reporters={"Grid": get_grid})
        #self.pois_at_play = 0

        # Crear BOMBEROS y colocarlos en la cuadrícula
        for id in range(len(SPAWNEO_BOMBEROS)):
            posicion = SPAWNEO_BOMBEROS[id]
            a = BomberoAgent(id, self, posicion)
            self.grid.place_agent(a, posicion)
            self.grid_state[posicion] = 12
            self.schedule.add(a)
            FILA_DE_BOMEROS.append(a)

        # Crear FUEGOS y colocarlos en la cuadrícula
        for fire_coor in SPAWNEO_FIREMARKERS: 
            self.grid_state[fire_coor] = 7

        # Crear POIs y colocarlos en la cuadrícula
        for spawn in SPAWNEO_POIS:  # 3 POIs iniciales
            coor = (spawn.red_coor, spawn.black_coor)
            self.grid_state[coor] = 1

        # w*w = ESQUINAS
        for j in range(1, 15, 2):
            for k in range(1, 19, 2):
                coor = (j,k)
                self.grid_state[coor] = 9 

        # n*n = PAREDES DE SPAWN
        for celda in SPAWNEO_PAREDES:
            self.grid_state[celda] = 9

        # n*w = PUERTAS
        for celda in SPAWNEO_PUERTAS:
            self.grid_state[celda] = 10 

        # remove exoteriores
        for celda in SPAWNEO_EXTERIORES:
            self.grid_state[celda] = 8
       
    def step(self):
        """Ejecuta un paso de la simulación."""
        self.datacollector.collect(self)
        self.schedule.step()

    def poi_refill(self):
        global POOL_POIS # para prevenir errores
        while len(SPAWNEO_POIS) < 3 and POOL_POIS:
            x, y = (random.randint(1, 6) * 2, random.randint(1, 8) * 2)
            while self.grid_state[(x,y)] > 5:
                x, y = (random.randint(1, 6) * 2, random.randint(1, 8) * 2)
            # el pop a la pool de pois solo ocurre aqui
            poi_type = (random.choice(POOL_POIS))
            if poi_type == "v":
                POOL_POIS.pop()
            else:
                POOL_POIS.pop(0)
            self.grid_state[(x,y)] = 2
            SPAWNEO_POIS.append(POI(x, y, poi_type))
            #print("Poi añadido: " + str(x) + ", " + str(y) + ", " + poi_type)

    def roll_midstep_flame(self):
        global LC_VICTIMAS
        x, y = (random.randint(1, 6) * 2, random.randint(1, 8) * 2)
        while self.grid_state[(x,y)] > 5:
            x, y = (random.randint(1, 6) * 2, random.randint(1, 8) * 2)

        coor = (x,y)
        coor_value = self.grid_state[coor]

        if coor_value < 3: # anything -> Smoke
            self.grid_state[coor] += 3
            # id.value 7 = Smoke + Agent

        elif coor_value < 8:  # anything+Smoke -> Fire
            
            # id.value 4 = Smoke + POI
            if coor_value == 4:
                for bombero in FILA_DE_BOMEROS:
                    if bombero.target_cell == coor:
                        if bombero.poi_aux == 'v': # the smoke+Poi was victim
                            LC_VICTIMAS += 1 # kills victim  
            elif coor_value == 5:
                LC_VICTIMAS += 1
                for bombero in FILA_DE_BOMEROS:
                    if bombero.pos == coor:
                        self.grid.move_agent(bombero, coor)
            elif coor_value == 6:
                for bombero in FILA_DE_BOMEROS:
                    if bombero.pos == coor:
                        self.grid.move_agent(bombero, coor)

            # (caso base) id.value 3 = Smoke
            self.grid_state[coor] = 7
        else: 
            # EXPLOSION
            abc = 123

    def winCondition(self):
        if WC_VICTIMAS >= 7:
            print("Se han rescatado 7 victimas!")
            return True
        elif LC_VICTIMAS >= 4:
            print("Se han perdido 4 victimas!")
            return True
        return False


# Función auxiliar para leer set de numeros de paredes_AUX
def read_number_set():
    number_set = []
    paredes_AUX_aux = []
    with open('setDeNumeros.txt', 'r') as file:
        lines = file.readlines()
    i_aux = 1
    for line in lines:
        string = line.strip().split() # xxxx xxxx xxxx -> xxxx, xxxx, xxxx
        if i_aux < 7: # PUERTAS
            for number in string: # xxxx -> x,x,x,x
                digits = [int(digit) for digit in number] # x,x,x,x -> x
                number_set.append(digits)
        elif i_aux < 10: # POIS
            if string[2] == 'v':
                POOL_POIS.pop()
            else: 
                POOL_POIS.pop(0)
            a, b, c = string
            a = int(a) * 2
            b = int(b) * 2
            poi = POI(a,b,c)
            SPAWNEO_POIS.append(poi)
        elif i_aux < 20: # FIRE MARKERS
            a, b = string
            a = int(a) * 2
            b = int(b) * 2
            SPAWNEO_FIREMARKERS.append((a, b))
        elif i_aux < 28: # PUERTAS INTERIORES
            a, b, c, d = string
            red_coor = int(a) + int(c)
            black_coor = int(b) + int(d)
            SPAWNEO_PUERTAS.append((red_coor, black_coor))
        elif i_aux < 32: # PUERTAS EXTERIORES
            a, b = string
            a = int(a) # red_coor
            b = int(b) # black_coor
            if a == 1: 
               b *= 2
            elif b == 1: 
                a *= 2
            elif b == 8: 
                b = (b*2) + 1
                a *= 2
            else : 
                a = (a*2) + 1
                b *= 2
            SPAWNEO_EXTERIORES.append((a, b))
        i_aux += 1
    for i in range(48): # [ xxxx, xxxx ] -> [x,x,x,x] # 0 to 47: 0-7, 8-15, 16-23 , 24-31 , 32-39 , 40-47 (6 rows)   
        # Consigue la coordenada de la celda de la que se obtienen las paredes_AUX
        dimension_down = (1 + int(i/8)) * 2 # correct
        dimension_right = (1 + (i%8)) * 2 # correct
        if number_set[i][0] == 1:
            a = (dimension_down-1, dimension_right) # up
            paredes_AUX_aux.append(a)
        if number_set[i][1] == 1:
            a = (dimension_down, dimension_right-1) # left
            paredes_AUX_aux.append(a)
        if number_set[i][2] == 1:
            a = (dimension_down+1, dimension_right) # down 
            paredes_AUX_aux.append(a)
        if number_set[i][3] == 1:
            a = (dimension_down, dimension_right+1) # right
            paredes_AUX_aux.append(a)
    paredes_AUX_aux = set(paredes_AUX_aux) # drop duplicates papa
    for pared in paredes_AUX_aux: # array de paredes_AUX en PAREDES_AUX[]
        SPAWNEO_PAREDES.append(pared)
read_number_set()

# Parámetros de renderización
MAX_GENERATIONS = 1000
COLOR = 12
HEIGTH = 15
WIDTH = 19

# id.value 0 = Empty
# id.value 1 = POI
# id.value 2 = Agent + Victim
# id.value 3 = Smoke
# id.value 4 = Smoke + POI
# id.value 5 = Smoke + Agent + Victim
# id.value 6 = Smoke + Agent
# id.value 7 = Fire
# id.value 8 = Puerta Exterior
# id.value 9 = Pared
# id.value 10 = Puerta
# id.value 11 = 
# id.value 12 = Agent

def build_json(i):
    all_grid = model.datacollector.get_model_vars_dataframe()
    data = all_grid.iloc[i, 0]
    puntos_interes_list = []
    red_coor = 0

    for item in data:  # row
        black_coor = 1
        red_coor += 1
        for subitem in item:
            puntos_interes_list.append({
                "row": int(red_coor),
                "col": int(black_coor),
                "type": int(subitem)
            })
            black_coor += 1
    config_data = {"puntos_interes": puntos_interes_list}
    return config_data # NO TOCAR ESTA LINEA

start_time = time.time()
model = BomberoModel(HEIGTH, WIDTH)
pasos_empleados_para_limpiar_todo = 0

CURRENT_JSON = json.dumps("xd").encode('utf-8')

# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final

for i in range(MAX_GENERATIONS):
    pasos_empleados_para_limpiar_todo += 1
    model.step()
    bombero = FILA_DE_BOMEROS.pop(0)
    FILA_DE_BOMEROS.append(bombero)
    bombero.step()
    model.roll_midstep_flame()

    print("Current step: " + str(i) + " has: " + str(WC_VICTIMAS) + " victims saved, and " + str(LC_VICTIMAS) + " victims lost.")
    #CURRENT_JSON = json.dumps(build_json(i)).encode('utf-8')
    #print(CURRENT_JSON)
    if model.winCondition():
        model.step()
        pasos_empleados_para_limpiar_todo += 1
        MAX_GENERATIONS = pasos_empleados_para_limpiar_todo
        break


# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final
# Procesamiento Final

print(f"Steps totales: {pasos_empleados_para_limpiar_todo}")
print('Tiempo de ejecución:', str(datetime.timedelta(seconds=(time.time() - start_time))))

all_grid = model.datacollector.get_model_vars_dataframe()

# Graficamos la información usando `matplotlib`
fig, ax = plt.subplots(figsize=(7, 7))
ax.set_xticks([])
ax.set_yticks([])
data = all_grid.iloc[0, 0]
im = ax.imshow(data, cmap=plt.cm.tab10, vmin=0, vmax=COLOR)

# Añadir los bordes de las celdas
WIDTH, HEIGTH = data.shape
for i in range(WIDTH):
    for j in range(HEIGTH):
        rect = plt.Rectangle((j - 0.5, i - 0.5), 1, 1, linewidth=1, edgecolor='black', facecolor='none')
        ax.add_patch(rect)

def animate(i):
    data = all_grid.iloc[i, 0]
    im.set_data(data)
    im.set_clim(vmin=0, vmax=COLOR)

anim = animation.FuncAnimation(fig, animate, frames=MAX_GENERATIONS, repeat=False)
plt.show()