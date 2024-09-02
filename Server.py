import json
from http.server import BaseHTTPRequestHandler, HTTPServer
import logging

class Server(BaseHTTPRequestHandler):

    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()

    def do_GET(self):
        self._set_response()
        self.wfile.write("GET request for {}".format(self.path).encode('utf-8'))

    def do_POST(self):
        try:
            # Leer y procesar el archivo config_inicial.txt
            with open('config_inicial.txt', 'r') as file:
                lines = file.readlines()
            
            # Procesar las primeras 6 líneas
            celdas = []
            for i in range(6):
                fila = lines[i].strip().split()
                celdas.append(fila)  # Mantener cada grupo de 4 dígitos como una cadena de texto

            # Procesar los puntos de interés (3 líneas siguientes)
            puntos_interes = []
            for i in range(6, 9):
                r, c, tipo = lines[i].strip().split()
                puntos_interes.append({"row": int(r), "col": int(c), "type": tipo})
            
            # Procesar los marcadores de fuego (10 líneas siguientes)
            fuego = []
            for i in range(9, 19):
                r, c = lines[i].strip().split()
                fuego.append({"row": int(r), "col": int(c)})

            # Procesar las puertas (8 líneas siguientes)
            puertas = []
            for i in range(19, 27):
                r1, c1, r2, c2 = lines[i].strip().split()
                puertas.append({"r1": int(r1), "c1": int(c1), "r2": int(r2), "c2": int(c2)})

            # Procesar los puntos de entrada (4 líneas finales)
            entradas = []
            for i in range(27, 31):
                r, c = lines[i].strip().split()
                entradas.append({"row": int(r), "col": int(c)})

            # Crear el diccionario que contiene todos los datos
            config_data = {
                "celdas": celdas,
                "puntos_interes": puntos_interes,
                "fuego": fuego,
                "puertas": puertas,
                "entradas": entradas
            }

            print("Configuración enviada:", config_data)  # Agregar este print para ver la configuración en la consola del servidor

            # Convertir la estructura a JSON y enviarla en la respuesta
            self._set_response()
            self.wfile.write(json.dumps(config_data).encode('utf-8'))

        except Exception as e:
            # Manejo de errores en caso de excepciones
            self.send_response(500)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(f"Internal server error: {str(e)}".encode('utf-8'))

def run_server(server_class=HTTPServer, handler_class=Server, port=8585):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info("Starting httpd...\n")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    httpd.server_close()
    logging.info("Stopping httpd...\n")

if __name__ == '__main__':
    from sys import argv
    
    if len(argv) == 2:
        run_server(port=int(argv[1]))
    else:
        run_server()