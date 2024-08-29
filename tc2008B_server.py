# TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
# Python server to interact with Unity via POST
# Sergio Ruiz-Loza, Ph.D. March 2021

from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json

def parse_map(file_path):
    with open(file_path, 'r') as file:
        lines = file.readlines()

    # Parse grid
    grid = [line.strip().split() for line in lines[:6]]

    # Parse points of interest
    poi = [line.strip().split() for line in lines[6:9]]
    poi = [{'row': int(x[0]), 'col': int(x[1]), 'type': x[2]} for x in poi]

    # Parse fire markers
    fires = [line.strip().split() for line in lines[9:19]]
    fires = [{'row': int(x[0]), 'col': int(x[1])} for x in fires]

    # Parse doors
    doors = [line.strip().split() for line in lines[19:27]]
    doors = [{'r1': int(x[0]), 'c1': int(x[1]), 'r2': int(x[2]), 'c2': int(x[3])} for x in doors]

    # Parse entry points
    entries = [line.strip().split() for line in lines[27:31]]
    entries = [{'row': int(x[0]), 'col': int(x[1])} for x in entries]

    return {'grid': grid, 'poi': poi, 'fires': fires, 'doors': doors, 'entries': entries}


class Server(BaseHTTPRequestHandler):

    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.end_headers()
        
    def do_GET(self):
        self._set_response()
        self.wfile.write("GET request for {}".format(self.path).encode('utf-8'))

    def do_POST(self):
        map_data = parse_map('config_inicial.txt')
        self._set_response()
        self.wfile.write(json.dumps(map_data).encode('utf-8'))

    
def run(server_class=HTTPServer, handler_class=Server, port=8585):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info("Starting httpd...\n") # HTTPD is HTTP Daemon!
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:   # CTRL+C stops the server
        pass
    httpd.server_close()
    logging.info("Stopping httpd...\n")

if __name__ == '__main__':
    from sys import argv
    
    if len(argv) == 2:
        run(port=int(argv[1]))
    else:
        run()



