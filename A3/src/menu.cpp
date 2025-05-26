#include "menu.h"

// Button 4 is the down button
// Button 3 is the up button
// Button 0 is the select button

static MenuOption menuOptions[10]; // Max 10 options
static int menuOptionCount = 0;
static int selectedIndex = 0;
static bool buttonPressed = false;
static bool confirmScreen = false;
static void (*menuSelectCallback)(int) = nullptr;

void setMenuOptions(const MenuOption* options, int count) {
    menuOptionCount = (count > 10) ? 10 : count;
    for (int i = 0; i < menuOptionCount; ++i) {
        menuOptions[i] = options[i];
    }
    selectedIndex = 0;
}

int getSelectedIndex() {
    return selectedIndex;
}

const MenuOption& getSelectedOption() {
    return menuOptions[selectedIndex];
}

void setMenuSelectCallback(void (*callback)(int)) {
    menuSelectCallback = callback;
}

void menuInit(MKRIoTCarrier& carrier) {
    carrier.display.init(64, 64);
}

void menuUpdate(MKRIoTCarrier& carrier) {
    carrier.display.fillScreen(carrier.display.color565(0, 0, 0));
    carrier.display.setCursor(0, 0);
    if (menuOptionCount == 0) {
        carrier.display.println("No orders found");
        return;
    }
    if (!confirmScreen) {
        for (int i = 0; i < menuOptionCount; ++i) {
            if (i == selectedIndex) {
                carrier.display.print("> ");
            } else {
                carrier.display.print("  ");
            }
            carrier.display.println(menuOptions[i].text);
        }

        carrier.Buttons.update();
        // Down button
        if (carrier.Buttons.getTouch(TOUCH4) && !buttonPressed) {
            buttonPressed = true;
            selectedIndex = (selectedIndex + 1) % menuOptionCount;
        } 
        // Up button
        else if (carrier.Buttons.getTouch(TOUCH3) && !buttonPressed) {
            buttonPressed = true;
            selectedIndex = (selectedIndex - 1 + menuOptionCount) % menuOptionCount;
        }
        // Select button
        else if (carrier.Buttons.getTouch(TOUCH0) && !buttonPressed) {
            buttonPressed = true;
            confirmScreen = true;
        }
        // Button released
        else if (!carrier.Buttons.getTouch(TOUCH4) && !carrier.Buttons.getTouch(TOUCH3) && !carrier.Buttons.getTouch(TOUCH0) && buttonPressed) {
            buttonPressed = false;
        }
    } else {
        // Confirm/Deny screen
        carrier.display.setCursor(0, 0);
        carrier.display.println("Confirm selection?");
        carrier.display.println(menuOptions[selectedIndex].text);
        carrier.display.println("");
        carrier.display.println("Up = Confirm");
        carrier.display.println("Down = Cancel");

        carrier.Buttons.update();
        // Up button = Confirm
        if (carrier.Buttons.getTouch(TOUCH3) && !buttonPressed) {
            buttonPressed = true;
            Serial.println("Confirmed selection");
            if (menuSelectCallback) {
                menuSelectCallback(selectedIndex);
            }
            confirmScreen = false;
        }
        // Down button = Cancel
        else if (carrier.Buttons.getTouch(TOUCH4) && !buttonPressed) {
            buttonPressed = true;
            Serial.println("Selection cancelled");
            confirmScreen = false;
        }
        // Button released
        else if (!carrier.Buttons.getTouch(TOUCH4) && !carrier.Buttons.getTouch(TOUCH3) && buttonPressed) {
            buttonPressed = false;
        }
    }
}
