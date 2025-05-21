#include <DS18B20.h>

int motor1pin1 = 2;
int motor1pin2 = 3;
DS18B20 ds(A1);

void setup() {
  Serial.begin(9600);
  pinMode(motor1pin1, OUTPUT);
  pinMode(motor1pin2, OUTPUT);
}

void loop() {
  float temp = ds.getTempC();
  Serial.print("Temperature: ");
  Serial.println(temp);

  if (temp > 30.0) {
    digitalWrite(motor1pin1,   HIGH);
  } else {

    digitalWrite(motor1pin1, LOW);
  }
}