#!/usr/bin/env python
from flask import Flask, jsonify, request
from flask_cors import CORS
import os
import RPi.GPIO as GPIO
import time

app = Flask(__name__)
CORS(app)


#interface for iot device
routes = [
    {
        'name': u"Voltage",
        'path': u"api/voltage/set",
        'method': u"POST",
        'type': u"slider"
    }
]

MIN_DUTY = 3
MAX_DUTY = 11
CENTRE = MIN_DUTY + (MAX_DUTY - MIN_DUTY) / 2

servo_pin = 5
duty_cycle = CENTRE     # Should be the centre for a SG90

# Configure the Pi to use pin names (i.e. BCM) and allocate I/O
GPIO.setmode(GPIO.BCM)
GPIO.setup(servo_pin, GPIO.OUT)

# Create PWM channel on the servo pin with a frequency of 50Hz
pwm_servo = GPIO.PWM(servo_pin, 50)
pwm_servo.start(duty_cycle)

@app.route('/')
def index():
    return u"See http://localhost:5000/api/info for more details"

### implementation of api functions ###
@app.route('/api/info')
def info():
    return jsonify(routes)

@app.route('/api/voltage/set', methods=['POST', 'OPTONS'])
def turn_led_on():
    req_data = request.get_json()
    val = float(req_data['data'])
    val = (val/100 * (11-3)) + 3
    if(val < 3) or (val > 11):
        return jsonify({"success": False})
    pwm_servo.start(val)
    return jsonify({"success": True})

ip = "10.42.0.35"

if __name__ == '__main__':
    app.run(host=ip, threaded=True)
