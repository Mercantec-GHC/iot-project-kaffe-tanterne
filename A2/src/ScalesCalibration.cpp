#include "HX711.h"

/*This calibration file is taken from an external source,
we have not written the code in this file,
the original writer of this file is Rob Tillaart. */

HX711 myScale;

//Adjust pins if needed.
uint8_t dataPin = 2;
uint8_t clockPin = 3;


void ScaleCalibrationStart()
{
  myScale.begin(dataPin, clockPin);
}

void calibrate()
{
  Serial.println("\n\nCALIBRATION\n===========");
  Serial.println("Remove all weight from the loadcell");
  while (Serial.available()) Serial.read();

  Serial.println("And press enter\n");
  while (Serial.available() == 0);

  Serial.println("Determine zero weight offset.");
  myScale.tare(20);
  int32_t offset = myScale.get_offset();

  Serial.print("OFFSET: ");
  Serial.println(offset);
  Serial.println();


  Serial.println("Place a weight on the loadcell.");
  while (Serial.available()) Serial.read();

  Serial.println("Enter the weight in (whole) grams and press enter.");
  uint32_t weight = 0;
  while (Serial.peek() != '\n')
  {
    if (Serial.available())
    {
      char ch = Serial.read();
      if (isdigit(ch))
      {
        weight *= 10;
        weight = weight + (ch - '0');
      }
    }
  }
  Serial.print("WEIGHT: ");
  Serial.println(weight);
  myScale.calibrate_scale(weight, 20);
  float scale = myScale.get_scale();

  Serial.print("SCALE:  ");
  Serial.println(scale, 6);

  Serial.print("\nuse scale.set_offset(");
  Serial.print(offset);
  Serial.print("); and scale.set_scale(");
  Serial.print(scale, 6);
  Serial.print(");\n");
  Serial.println("in the setup of your project");

  Serial.println("\n\n");
}

void ScaleCalibrationLoop()
{
  calibrate();
}