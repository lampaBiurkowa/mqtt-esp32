#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include <string.h>

WiFiClient espClient;
PubSubClient client(espClient);
const char *ssid = "name";
const char *password = "pass :D/";
const char *mqtt_broker = "test.mosquitto.org";
const char *input1Topic = "test-topic-student-pak-analog-1";
const char *input2Topic = "test-topic-student-pak-analog-2";
const char *ledTopic = "test-topic-student-pak-led";
const char *clientId = "3242432rffsg4weygwt3eF";
const int mqtt_port = 1883;

const int INPUT1_PIN = 34;
const int INPUT2_PIN = 35;
const int LED_PIN = 13;


void connectToWifi()
{
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED)
    {
        delay(1000);
        Serial.println("Waiting for WiFi connection");
    }
    Serial.print("Connected with ip: ");
    Serial.println(WiFi.localIP());
}

void connectMqtt()
{
    client.setServer(mqtt_broker, mqtt_port);
    client.setCallback(onReceive);
    WiFi.mode(WIFI_STA);
    while (!client.connected())
    {
        Serial.println("Connecting to mqtt...");
        if (!client.connect(clientId))
        {
            Serial.print("Failure code: ");
            Serial.print(client.state());
            delay(1000);
        }
    }
    client.subscribe(ledTopic);
}

void initializePins()
{
    pinMode(INPUT1_PIN, INPUT);
    pinMode(INPUT2_PIN, INPUT);
    pinMode(LED_PIN, OUTPUT);
}

void setup()
{
    Serial.begin(115200);
    connectToWifi();
    connectMqtt();
    initializePins(); 
}

void loop()
{
    uint8_t input1Value = analogRead(INPUT1_PIN);
    uint8_t input2Value = analogRead(INPUT2_PIN);
    client.publish(input1Topic, &input1Value, 1);
    client.publish(input2Topic, &input2Value, 1);
    client.loop();
    delay(1000);
}

void onReceive(char *topic, byte *payload, unsigned int length)
{
    if (strcmp(topic, ledTopic) != 0)
        return;

    Serial.print("Message:");
    for (int i = 0; i < length; i++)
        Serial.print((char)payload[i]);

    Serial.println("");
    StaticJsonDocument<256> doc;
    deserializeJson(doc, payload, length);
    if (doc["Blink"].as<uint8_t>() == 1)
    {
        digitalWrite(LED_PIN, HIGH);
        delay(500);
        digitalWrite(LED_PIN, LOW);
    }
}