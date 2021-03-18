# -*- encoding: UTF-8 -*-

import time
import argparse
from naoqi import ALProxy
import socket


def Recv():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    s.bind(('127.0.0.1', 7777))
    print("Bind UDP on prot:7777")
    data, addr = s.recvfrom(2048)

#    ShoulderRight_roll,ShoulderRight_pitch, ElbowRight_roll,ElbowRight_yaw= data.split(',')
    LaR,LaY,LsR,LsP,RaR,RaY,RsR,RsP,LhR,LhP,LkP,RhR,RhP,RkP,Ra,La,Ha= data.split(',')
    s.close()
#    return ShoulderRight_roll,ShoulderRight_pitch, ElbowRight_roll,ElbowRight_yaw
    return LaR,LaY,LsR,LsP,RaR,RaY,RsR,RsP,LhR,LhP,LkP,RhR,RhP,RkP,Ra,La,Ha
    #return ShoulderRight_roll, ShoulderRight_pitch

    '''
    while True:
     data, addr = s.recvfrom(1024)
     if  data:
       print(data)
       s.close()
       return data
     else:
        print("didn't catch one")
        '''




def main(robotIP, PORT=9559):
    motionProxy = ALProxy("ALMotion", robotIP, PORT)
    postureProxy = ALProxy("ALRobotPosture", robotIP, PORT)
    motionProxy.setStiffnesses("Head", 1.0)
    motionProxy.setStiffnesses("Body", 1.0)
    postureProxy.goToPosture("StandInit", 1.0)
    while(1):

      LaR, LaY, LsR, LsP, RaR, RaY, RsR, RsP, LhR, LhP, LkP, RhR, RhP, RkP,Ra,La,Ha=Recv()
# ShoulderRight_roll, ShoulderRight_yaw= Recv()
#   angles=[float(LaR),float(LaY),float(LsR),float(LsP),float(RaR),float(RaY),float(RsR),float(RsP),float(LhR),
#           float(LhP),float(LkP),float(RhR),float(RhP),float(RkP),]
      angles=[float(LaR),float(LaY),float(LsR),float(LsP),float(RaR),float(RaY),float(RsR),float(RsP),float(Ha)]
#,float(Ra),float(La) ,"RAnklePitch","LAnklePitch"
      names =["LElbowRoll","LElbowYaw","LShoulderRoll","LShoulderPitch","RElbowRoll","RElbowYaw","RShoulderRoll","RShoulderPitch"
,"HeadYaw"]


      '''
        names1 = "LElbowRoll"
        names2 = "LElbowYaw"
        names3 = "LShoulderRoll"
        names4 = "LShoulderPitch"
        names5 = "RElbowRoll"
        names6 = "RElbowYaw"
        names7 = "RShoulderRoll"
        names8 = "RShoulderPitch"
        names9 = "LHipRoll"
        names10 = "LHipPitch"
        names11= "LKneePitch"
        names12 = "RHipRoll"
        names13 = "RHipPitch"
        names14= "RKneePitch"
        names15=" RAnklePitch"
        names16=" LAnklePitch"
        
        
    limits = motionProxy.getLimits(names)
    if data < limits[0]:
        data=limits[0]
    else:
        if data > limits[1]:
            data=limits[1]

    fractionMaxSpeed = 0.1
    if ShoulderRight_pitch<-2.0857:
        ShoulderRight_pitch=-2.0857
    else:
        if ShoulderRight_pitch>2.0857:
            ShoulderRight_pitch=2.0857
    if ShoulderRight_roll<-1.3265:
        ShoulderRight_roll=-1.3265
    else:
        if ShoulderRight_roll>0.3142:
            ShoulderRight_roll=0.3142
    print(ShoulderRight_roll)
    print("       ")
    print(ShoulderRight_pitch)
    print("       ")
      '''

      fractionMaxSpeed = 0.2
      '''
    isEnabled  = True
    motionProxy.wbEnable(isEnabled)
    isEnable   = True
    supportLeg = "Legs"
    motionProxy.wbEnableBalanceConstraint(isEnable, supportLeg)
      '''
    #    effectorName = 'Head'
 #   isEnabled = True
#    motionProxy.wbEnableEffectorControl(effectorName, isEnabled)
      motionProxy.setAngles(names,angles, fractionMaxSpeed)
      '''
    motionProxy.setAngles(names1, [float (LaR)], fractionMaxSpeed)
    motionProxy.setAngles(names2, [float (LaY)], fractionMaxSpeed)
    motionProxy.setAngles(names3, [float (LsR)], fractionMaxSpeed)
    motionProxy.setAngles(names4, [float (LsP)], fractionMaxSpeed)
    motionProxy.setAngles(names5, [float (RaR)], fractionMaxSpeed)
    motionProxy.setAngles(names6, [float (RaY)], fractionMaxSpeed)
    motionProxy.setAngles(names7, [float (RsR)], fractionMaxSpeed)
    motionProxy.setAngles(names8, [float (RsP)], fractionMaxSpeed)
    motionProxy.setAngles(names9, [float (LhR)], fractionMaxSpeed)
    motionProxy.setAngles(names10,[float (LhP)], fractionMaxSpeed)
    motionProxy.setAngles(names11,[float (LkP)], fractionMaxSpeed)
    motionProxy.setAngles(names12,[float (RhR)], fractionMaxSpeed)
    motionProxy.setAngles(names13,[float (RhP)], fractionMaxSpeed)
    motionProxy.setAngles(names14,[float (RkP)], fractionMaxSpeed)
    motionProxy.setAngles(names14, [float(RkP)], fractionMaxSpeed)
    motionProxy.setAngles(names15, [float(Ra)], fractionMaxSpeed)
    motionProxy.setAngles(names16, [float(La)], fractionMaxSpeed)
      '''
#    motionProxy.setAngles(names4, [float (ElbowRight_yaw)], fractionMaxSpeed)
      time.sleep(1.0)
    postureProxy.goToPosture("SitRelax", 1.0)
#    isEnabled = False
 #   motionProxy.wbEnable(isEnabled)
#    effectorName = 'Head'
#    isEnabled = False
#    motionProxy.wbEnableEffectorControl(effectorName, isEnabled)




if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--ip", type=str, default="127.0.0.1",
                        help="Robot ip address")
    parser.add_argument("--port", type=int, default=9559,
                        help="Robot port number")

#    args = parser.parse_args()

    main("192.168.1.127", 9559)