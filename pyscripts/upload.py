import pandas as pd
from dotenv import load_dotenv
import shopify
import os
import asyncio

load_dotenv()

SHOPIFY_SHOP_URL = os.environ.get("SHOPIFY_SHOP_URL")
SHOPIFY_TOKEN = os.environ.get("SHOPIFY_TOKEN")
SHOPIFY_API_KEY = os.environ.get("SHOPIFY_API_KEY")
SHOPIFY_API_SECRET = os.environ.get("SHOPIFY_API_SECRET")
API_VERSION = "2022-01"

session = shopify.Session(SHOPIFY_SHOP_URL, API_VERSION, SHOPIFY_TOKEN)
shopify.ShopifyResource.activate_session(session)

def main():
    """
    docstring
    """
    df = pd.read_csv("./products_with_proper_sku.csv")
    df['FinalSKU'] = df['FinalSKU'].astype(str)
    df['FinalSKU'] = df['FinalSKU'].fillna('')
    df_products = df.groupby('Handle').first()
    df_variants = df[df['Title'].isna() | (df['Title'] == '')]

    with open("./missing.txt", 'w+t', newline='\n') as outfile:
        rproducts = shopify.Product.find(limit=250)

        while rproducts:
            for rproduct in rproducts:
                print(f"Working on {rproduct.title}...")
                for v in rproduct.variants:
                    try:
                        local_products = df_variants[(df_variants['Handle'] == rproduct.handle) & (df_variants['Option1 Value'] ==
                                                                                                   v.option1) & (df_variants['Option2 Value'] == v.option2) & (df_variants['Option3 Value'] == v.option3) & (df_variants['FinalSKU'] != '')]

                        if len(local_products) == 1:

                            lproduct = local_products.iloc[0]

                            final_sku = str(lproduct['FinalSKU']).strip()

                            if final_sku and final_sku != '' and v.sku != final_sku:
                                v.sku = final_sku
                                v.save()
                                print(
                                    f"SUCCESS: {rproduct.title} - {v.title} - {final_sku}")
                        else:
                            outfile.writelines(
                                [f"MISSING: {rproduct.title} - {v.title}\n"])
                    except Exception as e:
                        outfile.writelines(
                            [f"ERROR: {rproduct.title} - {v.title} - {str(e)}\n"])
                        print(
                            f"ERROR: {rproduct.title} - {v.title} - {str(e)}")

            if rproducts.has_next_page():
                rproducts = rproducts.next_page()
                continue

            break


async def get_done_items():
    """
    docstring
    """
    df = pd.read_excel("./products_export_1.xlsx")
    df_goods = df[df['Variant SKU'].str.endswith("-LT") | df['Variant SKU'].str.endswith(
        "-DK") | df['Variant SKU'].str.endswith("-BK") | df['Variant SKU'].str.endswith("-BL")]
    return df_goods['Variant SKU']


async def mainasync():
    """
    docstring
    """
    df = pd.read_excel("./products_with_proper_sku.xlsx")
    df['FinalSKU'] = df['FinalSKU'].astype(str)
    df['FinalSKU'] = df['FinalSKU'].fillna('')
    df = df.rename(columns={"Option1 Value": "Option1Value",
                   "Option2 Value": "Option2Value", "Option3 Value": "Option3Value"})
    df_products = df.groupby('Handle').first()
    df_variants = df[df['Title'].isna() | (df['Title'] == '')]

    config = read_config()
    done_items = get_done_items()

    for item in df_variants.itertuples():

        rproducts = shopify.Product.find(handle=item.Handle)
        if not rproducts:
            continue
        rproduct = rproducts[0]
        found = False
        for variant in rproduct.variants:
            if variant.option1 == item.Option1Value and variant.option2 == item.Option2Value and variant.option3 == item.Option3Value:
                found = True

                if variant.sku != item.FinalSKU and item.FinalSKU and item.FinalSKU != '' and item.FinalSKU != 'nan':
                    print(
                        f"Updating {rproduct.title} - {variant.title} from '{variant.sku}' to '{item.FinalSKU}'")
                    variant.sku = item.FinalSKU
                    variant.save()

                else:
                    print(
                        f"Skipping {rproduct.title} - {variant.title} with SKU: {item.FinalSKU}")

                break

        if not found:
            print(
                f"Not found {rproduct.title} - {variant.title}.")

if __name__ == "__main__":
    asyncio.run(mainasync())
