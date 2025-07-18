#include <DS18B20.h>

#ifndef WaterPump_h
#define WaterPump_h

extern DS18B20 ds;
extern float temp;
extern bool isDone;

void WaterPumpSetup();
void TemperatureRead();
void WaterPumpLoop();

#endif // WaterPump_h