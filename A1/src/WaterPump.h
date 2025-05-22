#include <DS18B20.h>

#ifndef WaterPump_h
#define WaterPump_h

DS18B20 ds(A1);
float temp = ds.getTempC();

void WaterPumpSetup();
void WaterPumpLoop();

#endif // WaterPump.h