from argparse import ArgumentParser
import os
import hashlib
from collections import defaultdict
import shutil
import csv

OUTPUT_FOLDER = '/Users/Warwick/Desktop/printfiles/'

# Create a dictionary to store files grouped by their checksums
checksum_to_files = defaultdict(list)

with open("./checksums.csv",'rt', newline='') as infile:
    reader = csv.DictReader(infile)
    for item in reader:
        checksum_to_files[item['CHECKSUM']].append(item['FILE'])

# Iterate through the dictionary and move unique files to the output directory
for checksum, files in checksum_to_files.items():
    unique_file_path = files[0]
    unique_file_name = os.path.basename(unique_file_path)
    output_path = os.path.join(OUTPUT_FOLDER, unique_file_name)
    shutil.copy(unique_file_path, output_path)
    print(f"Copied unique file: {unique_file_name} to {output_path}")


print("Unique files have been copied to the output directory.")