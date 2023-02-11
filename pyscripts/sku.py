
import shopify
import dotenv
import os
import csv
import pandas as pd

dotenv.load_dotenv()


if __name__ == "__main__":
    product_types = {}
    product_titles = {}
    with open("./products_export_1_populated.csv", 'wt', newline='', encoding='utf8') as outfile:
        with open("./products_export_1.csv", 'rt', newline='', encoding='utf8') as infile:
            reader = csv.reader(infile)
            for idx, row in enumerate(reader):
                if idx == 0:
                    header = row
                    fieldnames = row
                    fieldnames += ['New SKU']
                    writer = csv.DictWriter(outfile, fieldnames=fieldnames)
                    writer.writeheader()
                    continue

                item = {}
                for k, v in zip(header, row):
                    item[k.strip('\r\n\t ')] = v.strip('\r\n\t ') if v else ''

                if item['Type']:
                    product_types[item['Handle']] = item['Type']
                if item['Title']:
                    product_titles[item['Handle']] = item['Title']

                product_type = product_types.get(item['Handle'])
                product_title = product_titles.get(item['Handle'])

                if not product_type or not product_title:
                    item['New SKU'] = ''
                    writer.writerow(item)
                    continue

                parts = product_title.split(" ")
                if len(parts) >= 1:
                    product_title = ' '.join(parts[0:len(parts)-1])

                product_type = product_type.replace(" ", "")
                product_title = product_title.replace(" ", "")
                colour = item['Option1 Value'].replace(" ", "")
                fit = item['Option2 Value'].replace(" ", "")
                size = item['Option3 Value'].replace(" ", "")
                item['New SKU'] = f"{product_type}-{product_title}-{colour}-{fit}-{size}".upper()
                writer.writerow(item)
