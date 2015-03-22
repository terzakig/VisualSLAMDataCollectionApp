# VisualSLAMDataCollectionApp
An application for Data Collection of Video Sequences using the Flea3 USB3 Camera; GPS and IMU (UTC IMU02) readings 
are "framestamped" in separate files.

Employed APIs
----------
The application is making use of the PointGrey Flycapture 2 C++/C#/C API for camera configuration, frame capturing and recording. 
For other related image processing and storing including checkerboard calibration (see Flea3calibrator.cs), the OpenCV library is used 
(actually, the Managed version for C# known as Emgu).
The SiIMU02 communications interface was built from scratch.


Camera
------
The camera used is the Pointgrey Flea3 USB3 camera: Recording may be done in one of the following resolutions 
(and respective frame sampling rates):

1. 960x540 RGB, 24 fps (recommeded).
2. 1600x1200 RGB, 15 fps.
3. 1600x1200 YUV422, 30 fps.

GPS
---
The application reads NMEA formatted messages from the serial and keeps the "GPRMC" sentences. A class GPSreceiver 
(inherits NMEAReceiver) parses the sentence and retrieves, time, validity of signal , latitutde, longitude, 
speed over ground and course over ground. The readings become available in as public propertoes of the GPSReceiver class.

IMU
---
The IMU02 transmits a specifically formatted structure at 250 Hz over the RS232 line. The application samples these messages at 150 Hz 
(the signal is internally low-passed by the IMU FPGA to a 50Hz bandwidth) which is within safe distance from aliasing 
frequency bands (<100Hz). 

Output
------
Each recording generates 3 files, all named using the time and date in the following format:
<date>.<time>-000.avi  (video)
<date>.<time>.imu.txt  (imu)
<date>.<time>.gps.txt  (gps)

Example: 
13-01-2014.11-10-000.avi (video)
13-01-2014.11-10.imu.txt (imu)
13-01-2014.11-10.gps.txt (gps)

Files:
1. The video file (avi).

2. The GPS locations: Each line is marked with a frame identifier (actual frame counting starts from 1). The format is (comma delimited):

FrameIndex, character 'V'(Valid signal) or, 'I'(Invalid signal), Longitude (degrees), Latitude (degrees), Speed over ground (knots), Course over ground (degrees).

3. IMU readings: Again, the readings are framestamped and the format is:

FrameIndex, acceleration1, acceleration2, acceleration3, angular rate1, angular rate2, angular rate3

Note here that FrameIndex may assume the value 0, implying that the respective samples (IMU or GPS) have been received 
prior to the first frame capturing event.

The first two lines in the GPS and IMU contain the camera intrinsic parameters (1st line as 1x9 vector) and the 
distortion coefficients (2nd line as 1x4 vector) saved after the last calibration. 
By default (if no calibration was ever done), the identity matrix is used for intrinsics and 0 for each distortion coefficient.

