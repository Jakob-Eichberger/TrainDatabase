int const pinLeft = A1;
int const pinRight = A2;
double const val = 1000000.0;
int const waitCounter = 10;

void setup() {
  Serial.begin(9600);
  pinMode(pinLeft, INPUT);
  pinMode(pinRight, INPUT);
}

void loop() {
  if (pinOn(pinRight)) {
    getTime(pinLeft);
  } else if (pinOn(pinLeft)) {
    getTime(pinRight);
  }
}

bool pinOn(int pin) {
  return analogRead(pin) < 500;
}

void getTime(int pin2) {
  long start = micros();
  while (!pinOn(pin2)) {
    continue;
  }
  long end = micros();
  WaitForSensorClear(pin2);
  Serial.flush();
  Serial.print(String((end - start) / val, 22));
}

void WaitForSensorClear(int pin2) {
  int counter = 0;
  while (counter <= waitCounter) {
    delay(100);
    if (!pinOn(pin2))
      counter++;
  }
}