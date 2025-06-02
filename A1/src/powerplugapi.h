#ifndef POWERPLUGAPI_H
#define POWERPLUGAPI_H

#include "network.h"

class PowerPlugApi {
public:
    PowerPlugApi(Network& network, const char* socketAddress);
    bool toggleOn(); // Toggle the power plug
    bool toggleOff(); // Toggle the power plug off
private:
    Network& _network;
    const char* _socketAddress;
};

#endif // POWERPLUGAPI_H
