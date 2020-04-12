#!/usr/bin/env python
from flask import Flask, jsonify
import os
import LCD1602
import RPi.GPIO as GPIO
import time
from flask_cors import CORS

RoAPin = 11    # CLK Pin
RoBPin = 12    # DT Pin
BtnPin = 13    # Button Pin

globalCounter = 0

flag = 0
Last_RoB_Status = 0
Current_RoB_Status = 0

def setup():
    GPIO.setmode(GPIO.BOARD)       # Numbers GPIOs by physical location
    GPIO.setup(RoAPin, GPIO.IN, pull_up_down=GPIO.PUD_UP)    # input mode
    GPIO.setup(RoBPin, GPIO.IN, pull_up_down=GPIO.PUD_UP)
    GPIO.setup(BtnPin, GPIO.IN, pull_up_down=GPIO.PUD_UP)
    LCD1602.init(0x27, 1)    # init(slave address, background light)
    LCD1602.clear()
    LCD1602.write(0, 0, "Temperature is")
    LCD1602.write(0, 1, ' ')
    LCD1602.write(7, 1, 'degrees')
    time.sleep(2)

def printer(temp):
    if(temp < 0 or temp > 100):
	return
    if(temp < 10):
        temp = "  " + str(temp)
    elif(temp < 100):
        temp = " " + str(temp)
    LCD1602.write(3, 1, str(temp))

def rotaryDeal(channel):
    global flag
    global Last_RoB_Status
    global Current_RoB_Status
    global globalCounter
    Last_RoB_Status = GPIO.input(RoBPin)
    while(not GPIO.input(RoAPin)):
        Current_RoB_Status = GPIO.input(RoBPin)
        flag = 1
    if flag == 1:
        flag = 0
        if (Last_RoB_Status == 0) and (Current_RoB_Status == 1):
            globalCounter = globalCounter - 5
            if(globalCounter < 0):
                globalCounter = 0
        if (Last_RoB_Status == 1) and (Current_RoB_Status == 0):
            globalCounter = globalCounter + 5
            if(globalCounter > 100):
                globalCounter = 100
        printer(globalCounter)

def btnISR(channel):
    global globalCounter
    globalCounter = 0
    printer(globalCounter)

def destroy():
    GPIO.cleanup()      


app = Flask(__name__)
CORS(app)

#interface for iot device
routes = [
    {
        'name': u"Temperature",
        'path': u"api/temperature/get",
        'method': u"GET",
        'type': u"output"
    }
]

setup()
GPIO.add_event_detect(BtnPin, GPIO.FALLING, callback=btnISR)
GPIO.add_event_detect(RoAPin, GPIO.RISING, callback=rotaryDeal)


@app.route('/')
def index():
    return u"See http://localhost:5000/api/info for more details"



### implementation of api functions ###
@app.route('/api/info', methods=['GET', 'OPTIONS'])
def info():
    return jsonify(routes)

@app.route('/api/temperature/get', methods=['GET', 'OPTIONS'])
def turn_led_off():
    return jsonify({"data": str(globalCounter)})

ip = "10.42.0.227"

if __name__ == '__main__':
    app.run(host=ip,threaded=True)
