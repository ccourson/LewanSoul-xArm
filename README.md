# LewanSoul-xArm

## Objective

## USB Communications

## xArm Controller USB Protocol

test

| First Header  | Second Header |
| ------------- | ------------- |
| Content Cell  | Content Cell  |
| Content Cell  | Content Cell  |


Uses https://github.com/mikeobrien/HidLibrary



        ServoMove =             3,  // (byte)count (ushort)time { (byte)id (ushort)position }
        GroupRunRepeat =        5,  // (byte)group[255=all] (byte)times 
        GroupRun =              6,  // (byte)group (ushort)count[0=continuous]
        GroupStop =             7,  // -none-
        GroupErase =            8,  // (byte)group[255=all]
        GroupSpeed =            11, // (byte)group (ushort)percentage
        xServoOffsetWrite =      12, 
        xServoOffsetRead =       13, 
        xServoOffsetAdjust =     14,
        GetBatteryVoltage =     15, // -none-; (ushort)millivolts
        ServoOff =              20, // (byte)count { (byte)id }
        ServoPositionRead =     21, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
        ServoPositionWrite =    22, // (byte)count { (byte)id }
        ServoOffsetRead =       23, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
        ServoOffsetWrite =      24, // (byte)id (ushort)value
        BusServoMoroCtrl =      26, // (byte)id (byte)??? (ushort)speed
        BusServoInfoWrite =     27, // (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max
                                    //         (ushort)temp_max (byte)led_status (byte)led_warning
        BusServoInfoRead =      28  // -none-; (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max 
                                    //         (ushort)temp_max (byte)led_status (byte)led_warning (byte)dev_offset
                                    //         (ushort)pos (byte)temp (ushort)volt
