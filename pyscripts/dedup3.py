from argparse import ArgumentParser
import os
import hashlib
from collections import defaultdict
import shutil
import csv

ROOT_FOLDER = '/Users/Warwick/Desktop/printfiles/'
PNG_FOLDER = '/Users/Warwick/Desktop/printfiles/PNG/'
GCR_FOLDER = '/Users/Warwick/Desktop/printfiles/GCR/'


for file in os.listdir(ROOT_FOLDER):
    filepath = os.path.join(ROOT_FOLDER, file)
    
    if not os.path.isfile(filepath):
        continue
    
    _,ext = os.path.splitext(file)

    if ext.lower() == '.png':
        dest = PNG_FOLDER
    else:
        dest = GCR_FOLDER


    destfile = os.path.join(dest,file)
    shutil.move(filepath, dest)
    print(f"File moved from {filepath} to {destfile}")