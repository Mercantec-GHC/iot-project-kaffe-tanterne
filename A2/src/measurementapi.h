#ifndef MEASUREMENTAPI_H
#define MEASUREMENTAPI_H

#include "network.h"

class MeasurementApi {
public:
    MeasurementApi(Network& network, const char* host, const char* apiKey);
    int getMeasurements(char* buffer, int bufferSize);
    int updateMeasurement(int id, int value);
    int createMeasurement(const char* ingredientName, int value);
    int findMeasurementId(const char* buffer, const char* ingredientName);
private:
    Network& _network;
    const char* _host;
    const char* _apiKey;
};

#endif