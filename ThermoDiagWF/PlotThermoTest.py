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
# scan file for last thermo test occurrence
lineno = -1
startline = -1
for line in file_contents:
    lineno += 1
    if line.find("Thermo Diag starting!") > 0:
        startline = lineno
if startline < 0:
    print("Thermo test not found")
    exit()
# isolate date/time
eofdate = file_contents[startline].find("[")
dateTime = file_contents[startline][0:eofdate]
dateTime = dateTime.replace(",", ".")
startdt_obj = datetime.strptime(dateTime[:-5], '%Y-%m-%d %H:%M:%S')
milliseconds = int(dateTime[-4:-1])
startdt_obj = startdt_obj.replace(microsecond=milliseconds*1000)

# build lists of time, temps
tgt1 = "Temperatures= "

sampleno = 0
x = []
y = []
for i in range(10):
    y.append([])
accum = 0
nsamples = 0
for i in range(startline+1, len(file_contents)):
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
    x.append(sampTime)
    for i in range(len(ordinates)):
        y[i].append(myfloat(ordinates[i]))
    accum = accum+myfloat(ordinates[0])+myfloat(ordinates[3])
    nsamples = nsamples+1
# convert lists into numpy arrays
x = np.array(x)
y = np.array(y)
# generate plot
labels = ["Block", "Heatsink", "Ambient", "Block",
          "Heatsink", "Chiller", "IR", "IR", "External"]
if args[1] == "Left":
    plotThese = [0, 6, 8]
else:
    plotThese = [3, 7, 8]
deltaT = -999
if nsamples > 0:
    deltaT = accum/nsamples

fig = plt.figure()
ax = fig.add_subplot(111)
expectedx = [0, 180, 240]
expectedy = [20, 37, 37]
#ax.plot(expectedx, expectedy, linewidth=12,
#        color="green", alpha=0.3, label="Expected")
for i in plotThese:
    ax.plot(x, y[i, :], label=labels[i])
deltaT=(y[plotThese[2],:]-y[plotThese[1],:]).sum()
deltaT/=len(y[plotThese[1],:])

ax.set_title("ThermoTest " + args[1] + " " + dateTime)
ax.set(xlabel='Time [s]', ylabel='Temp [Degree C]')
plt.subplots_adjust(left=0.15)
plt.subplots_adjust(bottom=0.25)
plt.legend(loc='best')
fname = dateTime+'.png'
fname = fname.replace(" ", "_")
fname = fname.replace(":", "")
fname = fname.replace(",", "_")
offsetbox = TextArea(r'<$\Delta$T='+str(deltaT)+">")
xy = (x[plotThese[1]], y[plotThese[1],1])
ab = AnnotationBbox(offsetbox, xy,
                    xybox=(220, 50),
                    xycoords='data',
                    boxcoords="offset points",
                    arrowprops=dict(arrowstyle="->"),
                    bboxprops=dict(boxstyle="sawtooth"))
ax.add_artist(ab)

# save plot to file
plt.savefig(fname, format="png", bbox_inches='tight')

plt.show()


'''
# open
with open('original.json', 'r') as file:

    # read
    data = json.load(file)
    
    # add
    data["abc"]["mno"] = 3

    # remove
    data.pop("jkl")

    newData = json.dumps(data, indent=4)

# open
with open('modified.json', 'w') as file:

    # write
    file.write(newData)
'''