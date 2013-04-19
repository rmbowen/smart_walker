// Test the speed control PID loop using encoders

String readString;
#include <Servo.h> 
Servo myRightMotor;  // create servo object to control a servo 
Servo myLeftMotor;

int myRightMotorPin = 13;  
int myLeftMotorPin = 12;

int val;
int expectedEncoderTicks;
int RightEncoderPWR = 53;
int RightEncoderCHA = 51; //Right motor encoders, channel A
int RightEncoderCHB = 49; //Right motor encoders, channel B
int RightEncoderPos=0;
int RightEncoderCounter = 0;
int RightEncoderCountinLastSecond = 0;

int LeftEncoderPWR = 52;
int LeftEncoderCHA = 50; //Left motor encoders, channel A
int LeftEncoderCHB = 48; //Left motor encoders, channel B
int LeftEncoderPos = 0;
int LeftEncoderCounter = 0;
int LeftEncoderCountinLastSecond = 0;

int RightSpeed = 1550;
int LeftSpeed = 1550;
int maxReverseSpeed = 2000;
int minReverseSpeed = 1600;
int maxForwardSpeed = 900;
int minForwardSpeed = 1400;
int motorDirection; // 1 for forward, 0 for reverse


int RightEncoderCHA_PreviousVal; //Previous value of encoder to check if there was a change
int LeftEncoderCHA_PreviousVal;
int m = LOW;
int n = LOW;


int loopcount = 0;
unsigned long starttime;
unsigned long endtime;


void setup(){
  Serial.begin(9600); //Set baud rate to 9600

  
  myRightMotor.writeMicroseconds(1550); //set initial servo position at stop
  myRightMotor.attach(myRightMotorPin);  //the pin for the servo control 
  
  myLeftMotor.writeMicroseconds(1550); //set initial servo position at stop
  myLeftMotor.attach(myLeftMotorPin);  //the pin for the servo control 
  
  pinMode(RightEncoderPWR, OUTPUT);         
  digitalWrite(RightEncoderPWR, HIGH);       
  pinMode(LeftEncoderPWR, OUTPUT);         
  digitalWrite(LeftEncoderPWR, HIGH);   
  
  pinMode(RightEncoderCHA,INPUT);
  pinMode(RightEncoderCHB,INPUT);
  pinMode(LeftEncoderCHA,INPUT);
  pinMode(LeftEncoderCHB,INPUT);
  
  //Serial.println("Motor Control with Encoders Ready. Input one speed for the Right and Left Motors:"); // so I can keep track of what is loaded
  //motorDirection = 0; // 1 for forward, 0 for reverse
  RightSpeed = 1750; // 1000 to 1550 is forward
  LeftSpeed = 1750;  // 1550 to 2000 is reverse
  
  //myRightMotor.writeMicroseconds(RightSpeed); //set initial servo position at stop
  //myRightMotor.attach(13);  //the pin for the servo control 
  
  //myLeftMotor.writeMicroseconds(LeftSpeed); //set initial servo position at stop
  //myLeftMotor.attach(12);  //the pin for the servo control 

}

int input;
int sensorValue;
int rightExpectedTicks = 0;
int leftExpectedTicks = 0;

void loop(){
  //Testing to move forward at constant speed of 0.255m/s starting each motor at a servo value of 1700
  //expectedEncoderTicks = 1280; //per second
  //expectedEncoderTicks = 12; //per 10milliseconds
  
  while(Serial.available() == 0) { }

  input = Serial.read();
  
  while(Serial.available() == 0) { }
  Serial.read();
  
  DataRead:
  
  switch(input) {
    
    case 'L': // Left Force Sensor
      // read input from A4 pin - Left Force Sensor
      sensorValue = analogRead(A4);
      Serial.println(sensorValue);
      break;
    case 'R': // Right Force Sensor
      // read input from A5 pin - Right Force Sensor
      sensorValue = analogRead(A5);
      //printDouble(sensorValue, 1000);
      Serial.println(sensorValue);
      break;
    
    case 'N'://Navigation
     //while(Serial.available() != 0) {
       //delay(5000);
       
      while(Serial.available() == 0) {}
      //if (Serial.available() > 0) {
      String myString = String(Serial.parseInt());
      Serial.print("String Entered ");      
      Serial.println(myString);
      char charBuf[6];
      

      myString.toCharArray(charBuf, 6);
      
      for(int i=0; i<6; i++){
          Serial.print(charBuf[i]);
      }
      
      //Serial.print("First Num: ");
      //Serial.println(charBuf[0]);
      
      
      String mtr = String(charBuf[0]);
      char mtrBuf[2];
      mtr.toCharArray(mtrBuf, 2);
      mtrBuf[2] = 0;
      
      int first;
      first = atoi(mtrBuf);
      
      char secondBuf[3];
      
      String name1 = String(charBuf[1]);
      String name2 = String(charBuf[2]);

      String secondString = name1 + name2;
            
      //Serial.println("Mode");
      //Serial.println(charBuf[0]);
      
      char newBuf[3];
      secondString.toCharArray(newBuf, 3);
      newBuf[3] = 0;
      
      int second;
      second = atoi(newBuf);
      //Serial.println("Right Ticks");
      //Serial.println(second);
      
      String name3 = String(charBuf[3]);
      String name4 = String(charBuf[4]);
      String thirdString = name3 + name4;

      char thirdBuf[3];
      thirdString.toCharArray(thirdBuf, 3);
      thirdBuf[3] = 0;
      
      int third;
      third = atoi(thirdBuf);
      //Serial.println("Left Ticks");
      //Serial.println(third);
      
      motorDirection = first;
     //}
  
      rightExpectedTicks = second;
      leftExpectedTicks = third;

      //RightSpeed = 1750; // 1000 to 1550 is forward
      // LeftSpeed = 1750;  // 1550 to 2000 is reverse

      // Read whatever this thinks is in here???
      Serial.read();
      
      while(Serial.available() == 0) {   
 
          //Serial.print("MOO!");
       // if (rightExpectedTicks == 0) {
         //  myRightMotor.writeMicroseconds(1550);
        //}else {
        myRightMotor.writeMicroseconds(RightSpeed);
         // }
         // if (leftExpectedTicks == 0) {
         //    myLeftMotor.writeMicroseconds(1550); 
         // }else {
        myLeftMotor.writeMicroseconds(LeftSpeed);
         // }

        //Enable timers and counters
        starttime = micros();
        endtime = starttime;

        while((endtime - starttime) <= 10000){

          n = digitalRead(RightEncoderCHA); //take in latest value
          if((RightEncoderCHA_PreviousVal == LOW) && (n==HIGH)){ //check to see if the status has changed
            if(digitalRead(RightEncoderCHB) == LOW){ 
              RightEncoderPos++; //if the encoder is changing, and CHA is leading, the motor is moving reverse
            } else {
              RightEncoderPos--; //if the encode is changing, and CHB is leading, the motor is moving forward
            }
          }
          RightEncoderCHA_PreviousVal = n;
      
          m = digitalRead(LeftEncoderCHA); //take in latest value
          if((LeftEncoderCHA_PreviousVal == LOW) && (m==HIGH)){ //check to see if the status has changed
            if(digitalRead(LeftEncoderCHB) == LOW){ 
              LeftEncoderPos--; //if the encoder is changing, and CHA is leading, the motor is moving reverse
            } else {
              LeftEncoderPos++; //if the encode is changing, and CHB is leading, the motor is moving forward
            }
          }
          LeftEncoderCHA_PreviousVal = m;
  
          endtime = micros();
       } //end while loop of timer
  
        loopcount = loopcount + 1;
        
        RightEncoderCountinLastSecond = RightEncoderPos-RightEncoderCounter;
        LeftEncoderCountinLastSecond = LeftEncoderPos-LeftEncoderCounter;  
        RightEncoderCounter=RightEncoderPos;
        LeftEncoderCounter=LeftEncoderPos;
        
        /*
        Serial.print("Loop #: ");
        Serial.print("\t");
        Serial.print(loopcount, DEC);
        Serial.print("\t");
        Serial.print(expectedEncoderTicks);
        Serial.print("\t");
        Serial.print(RightEncoderCountinLastSecond);
        Serial.print("\t");
        Serial.print(LeftEncoderCountinLastSecond);
        Serial.print("\t");*/

        switch(motorDirection){
          case 1:
            rightExpectedTicks = abs(rightExpectedTicks);
            leftExpectedTicks = abs(leftExpectedTicks);
            //Check the actual speeds of the motors
            if(RightEncoderCountinLastSecond == rightExpectedTicks){
              /*Serial.print("RMG"); //right motor good
              Serial.print("\t");*/
            } 
            else if(RightEncoderCountinLastSecond > rightExpectedTicks)
            {
              if(RightSpeed > (minReverseSpeed)){
                RightSpeed = RightSpeed--; //reduce motor power
                /*Serial.print(RightSpeed);
                Serial.print("\t");*/
              }
            } 
            else if(RightEncoderCountinLastSecond < rightExpectedTicks)
            {
              if(RightSpeed < (maxReverseSpeed)){
                RightSpeed = RightSpeed++;//increase motor power only if max speed hasn't been approached
                /*Serial.print(RightSpeed);
                Serial.print("\t");*/
              }
            }
            
            if(LeftEncoderCountinLastSecond == leftExpectedTicks){
              /*Serial.print("LMG"); //left motor good
              Serial.println("");*/
            } 
            else if(LeftEncoderCountinLastSecond > leftExpectedTicks){
              if(LeftSpeed > (minReverseSpeed)){
                LeftSpeed = LeftSpeed--; //reduce motor power
                /*Serial.print(LeftSpeed);
                Serial.println("");*/
              }
            } 
            else if(LeftEncoderCountinLastSecond < leftExpectedTicks){
              if(LeftSpeed < (maxReverseSpeed)){
                LeftSpeed = LeftSpeed++;    //increase motor power
                /*Serial.print(LeftSpeed);
                Serial.println("");*/
              }
            }
            break;
            
          case 2:
            rightExpectedTicks = -rightExpectedTicks;
            leftExpectedTicks = -leftExpectedTicks;
            //Check the actual speeds of the motors
            if(RightEncoderCountinLastSecond == rightExpectedTicks){
              /*Serial.print("RMG"); //right motor good
              Serial.print("\t");*/
            } 
            else if(RightEncoderCountinLastSecond > rightExpectedTicks) //if moving too slow
              {
              if(RightSpeed > (maxForwardSpeed)){
                RightSpeed = RightSpeed--; //increase motor power
                /*Serial.print(RightSpeed);
                Serial.print("\t");*/
              }
              } 
            else if(RightEncoderCountinLastSecond < rightExpectedTicks)
            {
              if(RightSpeed < (minForwardSpeed)){
                RightSpeed = RightSpeed++;//decrease motor power only if max speed hasn't been approached
                /*Serial.print(RightSpeed);
                Serial.print("\t");*/
              }
            }
      
            if(LeftEncoderCountinLastSecond == leftExpectedTicks){
              /*Serial.print("LMG"); //left motor good
              Serial.println("");*/
            } 
            else if(LeftEncoderCountinLastSecond < leftExpectedTicks){
              if(LeftSpeed > (maxForwardSpeed)){
                LeftSpeed = LeftSpeed++; //reduce motor power
                /*Serial.print(LeftSpeed);
                Serial.println("");*/
              }
            } 
            else if(LeftEncoderCountinLastSecond > leftExpectedTicks){
              if(LeftSpeed < (minForwardSpeed)){
                LeftSpeed = LeftSpeed--;    //increase motor power
                /*Serial.print(LeftSpeed);
                Serial.println("");*/
              }
            }
            break;
           case 5:
             myRightMotor.writeMicroseconds(1550); //set initial servo position at stop
             myLeftMotor.writeMicroseconds(1550); //set initial servo position at stop
             while(Serial.available() == 0) { }

             input = Serial.read();
  
             while(Serial.available() == 0) { }
             Serial.read();
             goto DataRead;
             //break;
          }
       }
       break;
    }

} //end of main loop

  // prints val with number of decimal places determine by precision
  // NOTE: precision is 1 followed by the number of zeros for the desired number of decimial places
  // example: printDouble( 3.1415, 100); // prints 3.14 (two decimal places)
void printDouble( double val, unsigned int precision) {

  Serial.print (int(val));  //prints the int part
  Serial.print("."); // print the decimal point
  unsigned int frac;
  
  if(val >= 0)
    frac = (val - int(val)) * precision;
  else
     frac = (int(val)- val ) * precision;
     
  int frac1 = frac;
  
  while( frac1 /= 10 )
    precision /= 10;
    precision /= 10;
  while(  precision /= 10)
      Serial.print("0");

  Serial.println(frac,DEC) ;
}

