#!/usr/bin/env python
from flask import Flask, jsonify, request
import os
import LCD1602
import time
from flask_cors import CORS

def setup():
    LCD1602.init(0x27, 1)	# init(slave address, background light)
    LCD1602.write(0, 0, 'Greetings!!')
    LCD1602.write(1, 1, 'from SunFounder')
    time.sleep(5)

def printer(status, text):
    LCD1602.clear()
    LCD1602.write(0, 0, "Status: " + status)
    LCD1602.write(0,1, "Speed: " + text)


app = Flask(__name__)
CORS(app)


#interface for iot device
routes = [
    {
        'name': u"Start",
        'path': u"api/printer/start",
        'method': u"GET",
        'type': u"button"
    },
    {
        'name': u"Stop",
        'path': u"api/printer/stop",
        'method': u"GET",
        'type': u"button"
    },
    {
        'name': u"Speed",
        'path': u"api/printer/speed",
        'method': u"POST",
        'type': u"slider"
    }
]

setup()

@app.route('/')
def index():
    return u"See http://localhost:5000/api/info for more details"

status = False
value = 50

### implementation of api functions ###
@app.route('/api/info')
def info():
    return jsonify(routes)

@app.route('/api/printer/speed', methods=['POST'])
def print_value():
    req_data = request.get_json()
    value = req_data['data']
    printer(status, value)
    return jsonify({"success": True})

@app.route('/api/printer/start', methods=['GET'])
def start_button():
    status = True
    printer(status, value)
    return jsonify({"success": True})

@app.route('/api/printer/start', methods=['GET'])
def stop_button():
    status = False
    printer(status, value)
    return jsonify({"success": True})

ip = "10.42.0.55"

if __name__ == '__main__':
    app.run(host=ip, threaded=True)
