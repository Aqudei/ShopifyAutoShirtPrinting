
import shopify
import dotenv
import os
import csv
import pandas as pd

dotenv.load_dotenv()

product_types = {}
product_titles = {}
product_options_lookup = {}


def read_config():
    """
    docstring
    """
    config = {}

    config['types'] = {}
    df = pd.read_excel("./config.xlsx")
    for item in df.itertuples():
        config['types'][item.Key.upper().strip(
            "\r\n\t ")] = item.Value.strip("\r\n\t ")

    df = pd.read_excel("./config.xlsx", sheet_name=1)
    config['colours'] = {}
    for item in df.itertuples():
        config['colours'][item.Key.upper().strip(
            "\r\n\t ")] = item.Value.strip("\r\n\t ")

    df = pd.read_excel("./config.xlsx", sheet_name=2)
    config['fits'] = {}
    for item in df.itertuples():
        config['fits'][item.Key.upper().strip(
            "\r\n\t ")] = item.Value.strip("\r\n\t ")

    df = pd.read_excel("./config.xlsx", sheet_name=3)
    config['styles'] = {}
    for item in df.itertuples():
        config['styles'][item.Key.upper().strip(
            "\r\n\t ")] = item.Value.strip("\r\n\t ")

    return config


def sku_clean_up(sku):
    """
    docstring
    """
    import re

    return re.sub(r'\-+', '-', sku.replace("None", '')).strip("-\r\n\t ").replace("'", '').replace("`", '')


def parse_options(item: dict):
    """
    docstring
    """

    option_info = product_options_lookup.get(item['Handle'])
    if not option_info:
        return

    opts = {}


    for k, v in option_info.items():

        opts[k] = item[v]

    return opts


if __name__ == "__main__":
    config = read_config()
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

                if item['Title']:
                    product_titles[item['Handle']] = item['Title']
                    product_types[item['Handle']] = item['Type']
                    product_options_lookup[item['Handle']] = {}

                    if item.get('Option1 Name'):
                        product_options_lookup[item['Handle']
                                               ][item['Option1 Name']] = 'Option1 Value'

                    if item.get('Option2 Name'):
                        product_options_lookup[item['Handle']][
                            item['Option2 Name']] = 'Option2 Value'

                    if item.get('Option3 Name'):
                        product_options_lookup[item['Handle']][
                            item['Option3 Name']] = 'Option3 Value'

                product_option = parse_options(item)

                product_type = product_types.get(item['Handle'])
                product_title = product_titles.get(item['Handle'])

                if not product_type or not product_title:
                    item['New SKU'] = ''
                    writer.writerow(item)
                    continue

                parts = product_title.split(" ")
                if len(parts) >= 1:
                    product_title = ' '.join(parts[0:len(parts)-1])

                # Transform
                product_type = config['types'].get(product_type.upper())

                colour = ''
                if 'Colour' in product_option:
                    colour = config['colours'].get(
                        product_option['Colour'].upper())

                fit_or_style = ''
                if 'Fit' in product_option.keys():
                    fit_or_style = config['fits'].get(
                        product_option['Fit'].upper())
                elif 'Style' in product_option.keys():
                    fit_or_style = config['styles'].get(
                        product_option['Style'].upper())

                if not product_type:
                    print(
                        f"TYPE <{product_type}> NOT FOUND FOR PRODUCT '{item['Handle']}'")
                    item['New SKU'] = ''
                    writer.writerow(item)
                    continue

                product_title = product_title.replace(" ", "")
                size = product_option.get('Size', '').replace(" ", "")

                item['New SKU'] = sku_clean_up(
                    f"{product_type}-{product_title}-{colour}-{fit_or_style}-{size}".upper())

                writer.writerow(item)
