#ifndef MEASUREMENTAPI_H
#define MEASUREMENTAPI_H

#include "network.h"

class MeasurementApi {
public:
    MeasurementApi(Network& network, const char* host, const char* apiKey, const int apiPort);
    int getMeasurements(char* buffer, int bufferSize);
    int findMeasurementId(const char* buffer, const char* ingredientName);
    int findIngredientId(const char* buffer, const char* ingredientName);
    int createMeasurement(int ingredientId, int value);
    int updateMeasurement(int id, int ingredientId, int value);
    int createIngredient(const char* ingredientName);
    int getIngredients(char* buffer, int bufferSize);
private:
    Network& _network;
    const char* _host;
    const char* _apiKey;
    const int _apiPort;
};

#endif