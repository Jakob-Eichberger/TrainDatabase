import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(16,GPIO.IN)
GPIO.setup(18,GPIO.IN)
sensor1 = 16
sensor2 = 18

start =time.time()
end =time.time()
isNotDisposed = True

try: 
    while isNotDisposed:
        if(GPIO.input(sensor1) == False):
            start = time.time()
            while GPIO.input(sensor2) == True:
                continue
            end = time.time()
            print(end-start)
            while GPIO.input(sensor2) == False:
                time.sleep(2)
                continue
            isNotDisposed = False
        if(GPIO.input(sensor2) == False):
            start = time.time()
            while GPIO.input(sensor1) == True:
                continue
            end = time.time()
            print(end-start)
            while GPIO.input(sensor1) == False:
                time.sleep(2)
                continue
            isNotDisposed = False
except KeyboardInterrupt:
    GPIO.cleanup()
