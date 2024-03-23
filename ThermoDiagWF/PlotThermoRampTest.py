import numpy as np
import matplotlib.pyplot as plt
import sys
from datetime import datetime
from matplotlib.offsetbox import (TextArea, DrawingArea, OffsetImage,
                                  AnnotationBbox)

import json


def myfloat(s):
    result = None
    for i in s.split():
        try:
            result = float(i)
            break
        except:
            result = 0
            continue
    if result != None:
        if result < 0:
            return 0
    return result


if len(sys.argv) < 2:
    quit()
args = sys.argv
# load diag log into memory
file_path = 'diags.log'
with open(file_path, 'r') as file:
    file_contents = file.readlines()
# scan file for last 4 thermo test occurrences
lineno = -1
startline = []
elapsedTimes=[]
for line in file_contents:
    lineno += 1
    if line.find("Starting Temperature monitoring") > 0:
        startline.append(lineno)
    if line.find("elapsed time") > 0:
        elapsedTimes.append(lineno)
if len(startline) < 4:
    print("Thermo test not found")
    exit()
# isolate date/time
eofdate = file_contents[startline[0]].find("[")
dateTime = file_contents[startline[0]][0:eofdate]
dateTime = dateTime.replace(",", ".")
startdt_obj = datetime.strptime(dateTime[:-5], '%Y-%m-%d %H:%M:%S')
milliseconds = int(dateTime[-4:-1])
startdt_obj = startdt_obj.replace(microsecond=milliseconds*1000)

# build lists of time, temps
tgt1 = "Temperatures= "
startline.append(len(file_contents))
startline=startline[-6:]
elapsedTimes=elapsedTimes[-5:]

x1 = []
y1 = []
for i in range(10):
    y1.append([])
j=0
#left bay tests (ramp-up, ramp-down)
for i in range(startline[j]+1, startline[j+2]):
    index = file_contents[i].find(tgt1)
    if index < 0:
        continue
    sampeofdate = file_contents[i].find("[")
    sampdateTime = file_contents[i][0:eofdate]
    sampdateTime = sampdateTime.replace(",", ".")
    adt_obj = datetime.strptime(sampdateTime[:-5], '%Y-%m-%d %H:%M:%S')
    milliseconds = int(sampdateTime[-4:-1])
    adt_obj = adt_obj.replace(microsecond=milliseconds*1000)
    sampTime = (adt_obj-startdt_obj).total_seconds()
    index1 = file_contents[i].find(tgt1)
    if index1 < 0:
        continue
    temp = file_contents[i][index1+len(tgt1):]
    ordinates = temp.split(' ', 10)
    x1.append(sampTime)
    for i in range(len(ordinates)):
        y1[i].append(myfloat(ordinates[i]))

# convert lists into numpy arrays
x1 = np.array(x1)
y1 = np.array(y1)
# generate plot

fig = plt.figure(figsize=(8,8))
ax = fig.add_subplot(311)
ax.plot(x1/100, y1[0, :])

ax.set_title("Thermo Left Ramp Test " + args[1] + " " + dateTime)
ax.set(xlabel='Time [s]', ylabel='Temp [Degree C]')
plt.subplots_adjust(left=0.15)
plt.subplots_adjust(bottom=0.25)
plt.legend(loc='best')
fname = dateTime+'.png'
fname = fname.replace(" ", "_")
fname = fname.replace(":", "")
fname = fname.replace(",", "_")
# place a text box in upper left in axes coords
ix=file_contents[elapsedTimes[0]].find("elapsed time=")
etime=int(file_contents[elapsedTimes[0]][ix+len("elapsed time="):])/1000
if etime<200: 
    testResult="Pass"
else:
    testResult="Fail"

ix1=file_contents[elapsedTimes[1]].find("elapsed time=")
ax.text(0.05, -0.3, "Ramp Up "+ str(etime)+" s : "+testResult, transform=ax.transAxes, fontsize=14,
        verticalalignment='top')
etime=int(file_contents[elapsedTimes[1]][ix+len("elapsed time="):])/1000      
if etime<700: 
    testResult="Pass"
else:
    testResult="Fail"
ax.text(0.05, -0.45, "Ramp Down " + str(etime)+" s : "+testResult, transform=ax.transAxes, fontsize=14,
        verticalalignment='top')

x2 = []
y2 = []
for i in range(10):
    y2.append([])
j=2 #right bay tests (ramp-up, ramp-down)
for i in range(startline[j]+1, startline[j+2]):
    index = file_contents[i].find(tgt1)
    if index < 0:
        continue
    sampeofdate = file_contents[i].find("[")
    sampdateTime = file_contents[i][0:eofdate]
    sampdateTime = sampdateTime.replace(",", ".")
    adt_obj = datetime.strptime(sampdateTime[:-5], '%Y-%m-%d %H:%M:%S')
    milliseconds = int(sampdateTime[-4:-1])
    adt_obj = adt_obj.replace(microsecond=milliseconds*1000)
    sampTime = (adt_obj-startdt_obj).total_seconds()
    index1 = file_contents[i].find(tgt1)
    if index1 < 0:
        continue
    temp = file_contents[i][index1+len(tgt1):]
    ordinates = temp.split(' ', 10)
    x2.append(sampTime)
    for i in range(len(ordinates)):
        y2[i].append(myfloat(ordinates[i]))

# convert lists into numpy arrays
x2 = np.array(x2)
y2 = np.array(y2)
# generate plot

ax = fig.add_subplot(313)
ax.plot(x2/100, y2[3, :])

ax.set_title("Thermo Right Ramp Test " + args[1] + " " + dateTime)
ax.set(xlabel='Time [s]', ylabel='Temp [Degree C]')
plt.subplots_adjust(left=0.15)
plt.subplots_adjust(bottom=0.25)
plt.legend(loc='best')
fname = dateTime+'.png'
fname = fname.replace(" ", "_")
fname = fname.replace(":", "")
fname = fname.replace(",", "_")
# place a text box in upper left in axes coords
ix=file_contents[elapsedTimes[0]].find("elapsed time=")
etime=int(file_contents[elapsedTimes[2]][ix+len("elapsed time="):])/1000
if etime<200: 
    testResult="Pass"
else:
    testResult="Fail"

ix1=file_contents[elapsedTimes[1]].find("elapsed time=")
ax.text(0.05, -0.3, "Ramp Up " + str(etime)+" s : "+testResult, transform=ax.transAxes, fontsize=14,
        verticalalignment='top')
etime=int(file_contents[elapsedTimes[3]][ix+len("elapsed time="):])/1000      
if etime<700: 
    testResult="Pass"
else:
    testResult="Fail"
ax.text(0.05, -0.45, "Ramp Down " + str(etime)+" s : "+testResult, transform=ax.transAxes, fontsize=14,
        verticalalignment='top')

# save plot to file
plt.savefig(fname, format="png", bbox_inches='tight')

plt.show()
