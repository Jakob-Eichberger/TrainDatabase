import RPi.GPIO as GPIO
import time
import sys

sensor1 = 16
sensor2 = 18

GPIO.setmode(GPIO.BOARD)
GPIO.setup(sensor1, GPIO.IN)
GPIO.setup(sensor2, GPIO.IN)

start = time.time()
end = time.time()
itterations = 5
def test(sensorA, sensorB):
    if(GPIO.input(sensorB) == False):
        start = time.time()
        while GPIO.input(sensorA) == True:
            continue
        end = time.time()
        counter = 0
        while counter<=5:
            time.sleep(1)
            if(GPIO.input(sensorA) == True):
                counter  = counter+1
        print(end-start)
        sys.exit(0)

try:
    try:
        itterations = sys.argv[1] 
    except:
        print("Missing itteration arguments! (using default 5))")
    while True:
        test(sensor2,sensor1 )
        test(sensor1, sensor2)
except:
    GPIO.cleanup()
