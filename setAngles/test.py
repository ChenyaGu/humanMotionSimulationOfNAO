# -*- coding: utf-8 -*-
"""
Created on Sun Dec 10 08:53:38 2017

@author: lulu
"""

import socket

s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

s.bind(('127.0.0.1', 7777))
print("Bind UDP on prot:7777")

data, addr = s.recvfrom(2048)
print(data)

s.close()
