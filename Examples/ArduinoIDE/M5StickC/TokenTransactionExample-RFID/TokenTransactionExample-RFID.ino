#include <M5StickC.h>
#include <WiFi.h>
#include <Wire.h>
#include "MFRC522_I2C.h"
#include "HTTPClient.h"

// 0x28 is i2c address on SDA. Check your address with i2cscanner if not match.
MFRC522 mfrc522(0x28);   //Create MFRC522 instance.

String moduleName = "NEBLIO RFID";

// please fill IP address
String DEFAULT_SRVIP = "192.168.0.1"; // "IP OF PC WHERE YOU RUN VEconomy App"
String SERVER_PORT = "8080"; // "Port of VEconomy App 8080 is default port"

// please fill own network parameters
String ssid = "YOUR SSID NAME";//"YOUR SSID NAME";
String password = "YOUR WIFI PASSWORD";//"YOUR WIFI PASSWORD";

String pathBase = "http://" + DEFAULT_SRVIP + ":" + SERVER_PORT + "/api/";

byte mac[6];
String localMACAddr;
IPAddress ip;
WiFiClient espClient;

const int numberPoints = 7;
float wifiStrength;

void ClearDisplay(bool isBackToMain){
  M5.Lcd.fillRect(0,0,160,80,BLACK);
  M5.Lcd.setTextColor(BLUE);
  M5.Lcd.setCursor(60, 2); M5.Lcd.println("Neblio IoT Node");
  
  if (isBackToMain){

    //M5.Lcd.setCursor(0,15); M5.Lcd.printf(" Unit Name ");
    M5.Lcd.setTextSize(2);
    M5.Lcd.setCursor(5,17); M5.Lcd.printf((char*)moduleName.c_str());
    M5.Lcd.setTextSize(1);
    M5.Lcd.setCursor(0,45); M5.Lcd.printf(" WiFi Signal ");
    M5.Lcd.fillRect(0,60,70,15,BLACK);
    M5.Lcd.setCursor(0,60); M5.Lcd.printf(" RSSI: %.1f", wifiStrength);
  }
  
  M5.Lcd.setTextColor(WHITE);

}

void connectWifi() {
    ClearDisplay(false);
    M5.Lcd.setCursor(0,15); M5.Lcd.print("Connecting WIFI");
    M5.Lcd.setCursor(0,30); M5.Lcd.print(ssid);
    M5.Lcd.setCursor(0,45); 
    
    WiFi.mode(WIFI_STA);
    WiFi.disconnect();

    int attempts = 20;
    
    WiFi.begin((char*)ssid.c_str(), (char*)password.c_str());
    while (WiFi.status() != WL_CONNECTED) {
      M5.Lcd.setTextWrap(true);
      M5.Lcd.print(".");
      delay(500);
      
      attempts--;

      if (attempts <= 0){
        return;
      }
    }
    M5.Lcd.setTextWrap(false);
    
    ClearDisplay(false);
    M5.Lcd.setCursor(0,45);
    M5.Lcd.println("Connection done!");
    delay(1000);
    
    M5.Lcd.fillRect(0,0,160,80,BLACK);

    M5.Lcd.setCursor(0,15); M5.Lcd.printf("IP:"); 
    ip = WiFi.localIP();
    
    M5.Lcd.setCursor(20,15);M5.Lcd.printf("%d.%d.%d.%d", ip[0], ip[1], ip[2], ip[3]);
    M5.Lcd.setCursor(0,30); M5.Lcd.printf("MAC: %s", (char*)localMACAddr.c_str());
    delay(1000);
    ClearDisplay(true);
}

void setup() {
  // put your setup code here, to run once:
  M5.begin();
  M5.Lcd.setRotation(3);
  ClearDisplay(false);
  M5.Lcd.setTextColor(WHITE);
  delay(500);
  ClearDisplay(false);
  
  connectWifi();

  pinMode(M5_LED, OUTPUT);
  digitalWrite(M5_LED, HIGH);

  //setup RFID reader
  Wire.begin();                   // Initialize I2C
  mfrc522.PCD_Init();             // Init MFRC522
  //ShowReaderDetails();            // Show details of PCD - MFRC522 Card Reader details
  
  M5.Lcd.setCursor(0,30); M5.Lcd.print("Starting...");
  delay(500);
  
  ClearDisplay(true);
  
}

String uidToStr(const byte* uid)
{
  String result;
  for (int i = 0; i < 6; ++i) {
    result += String(uid[i], 16);
  }
  
  return result;
}

void loop() {
   if ( ! mfrc522.PICC_IsNewCardPresent() || ! mfrc522.PICC_ReadCardSerial() ) {
    delay(50);
    return;
  }
  
  // Dump UID
  Serial.print(F("RFID UID:"));
  M5.Lcd.println(" ");
  
  for (byte i = 0; i < mfrc522.uid.size; i++) {
    Serial.print(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
    Serial.print(mfrc522.uid.uidByte[i], HEX);
    //M5.Lcd.print(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
    //M5.Lcd.print(mfrc522.uid.uidByte[i], HEX);
  } 
  Serial.println();
  String uid = "";
  
  Serial.print("uid:");
  uid = uidToStr(mfrc522.uid.uidByte);
  Serial.print((char*)uid.c_str());

  HTTPClient http;

  /* Example of GET command which receive MQTT parameters
  String path = pathBase + "GetServerParams";
  http.begin(path.c_str());
  int httpResponseCode = http.GET();
  
  if (httpResponseCode>0) {
        Serial.print("HTTP Response code: ");
        Serial.println(httpResponseCode);
        String payload = http.getString();
        Serial.println(payload);
      }
      else {
        Serial.print("Error code: ");
        Serial.println(httpResponseCode);
      }
      // Free resources
  //http.end();
  */

  // example of PUT command to send Neblio NTP1 token transaction
  String path = pathBase + "SendNTP1Token";
  
  http.begin((char*)path.c_str());
  http.addHeader("accept", "*/*");
  http.addHeader("Content-Type", "application/json;charset=utf-8");

  // dont forget to unlock the account before sending transaction!
  // create token send request data
  String token = "{"
                   "\"ReceiverAddress\":\"NVSzTaaQuRukkLvQp1ZoeoaN6agYdGVX73\","
                   "\"SenderAddress\":\"NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA\","
                   "\"Symbol\":\"CART\","
                   "\"Id\": \"La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei\","
                   "\"Amount\": 1,"
                   "\"Metadata\":"
                      "{"
                        "\"ModuleName\":\"" + moduleName + "\","
                        "\"Data\":\"Neblio M5Stick RFID Reader. Readed RFID: " + uid + "\""
                      "}"
                  "}";
                  
  // print data to com port - just for debug
  Serial.print((char*)token.c_str());
  
  // send data in PUT request
  int httpResponseCode = http.PUT((char*)token.c_str());

  // parse output if code is not error
  if(httpResponseCode > 0 && httpResponseCode < 400){
    String response = http.getString();  
    Serial.println(httpResponseCode);
    Serial.println(response);   
  } 
  else {
    Serial.print("Error on sending PUT Request: ");
    Serial.println(httpResponseCode);
  }

  http.end();

  M5.update();
  // wait 10 seconds before allow new read
  delay(10000);
}
