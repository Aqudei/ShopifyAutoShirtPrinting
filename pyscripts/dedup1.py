from argparse import ArgumentParser
import os
import hashlib
from collections import defaultdict
import shutil
import csv

INPUT_FOLDER = '/Users/Warwick/'

# Function to calculate the checksum of a file
def calculate_checksum(file_path, block_size=65536):
    hasher = hashlib.sha256()  # You can choose a different hash algorithm if needed
    with open(file_path, 'rb') as f:
        while True:
            data = f.read(block_size)
            if not data:
                break
            hasher.update(data)
    return hasher.hexdigest()


# Create a dictionary to store files grouped by their checksums
checksum_to_files = defaultdict(list)

parser = ArgumentParser()
parser.add_argument("--folder")

args = parser.parse_args()

with open("./checksums.csv",'wt') as outfile:
    writer = csv.writer(outfile)
    writer.writerow(["FILE","CHECKSUM"])

    # Iterate through all files in the input directory
    for root, _, files in os.walk(INPUT_FOLDER):
        for file in files:
            _,ext  = os.path.splitext(file)
            
            if not ext.lower() in ['.gcr','.png']:
                continue
            
            file_path = os.path.join(root, file)
            print(f"Found file {file_path}")
            checksum = calculate_checksum(file_path)
            # checksum_to_files[checksum].append(file_path)
            writer.writerow([file_path,f"{checksum}"])

         