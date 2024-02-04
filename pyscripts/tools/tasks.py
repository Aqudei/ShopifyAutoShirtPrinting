from celery import shared_task
from django.conf import settings
from tools.models import Product, Variant, Backup
import shopify
import time
import logging
import gzip
import subprocess
import os
from django.utils import timezone
from decouple import config

logger = logging.getLogger(__name__)
print(__name__)


@shared_task(time_limit=None)
def fetch_products():
    """
    docstring
    """
    session = shopify.Session(
        settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
    shopify.ShopifyResource.activate_session(session)

    products = shopify.Product.find()

    while True:

        for product in products:
            logger.info(f"Importing {product.title}...")
            product_obj, created = Product.objects.update_or_create(
                shopify_id=product.id,
                defaults={
                    "handle": product.handle,
                    "product_type": product.product_type,
                    'title':product.title
                }
            )

            variants = product.variants
            for variant in variants:
                variant_data = variant.to_dict()
                
                image_src = ''
                if variant.image_id and variant.image_id > 0:
                    image = list([img for img in product.images if img.id==variant.image_id])
                else:
                    image = list([img for img in product.images])
                
                if image and len(image)>0:
                    image_src = image[0].src
                        
                variant_obj, created = Variant.objects.update_or_create(
                    shopify_id=variant_data['id'],
                    defaults={
                        "title": variant_data['title'],
                        "sku": variant_data['sku'],
                        "option1": variant_data.get('option1'),
                        "option2": variant_data.get('option2'),
                        "option3": variant_data.get("option3"),
                        "product": product_obj,
                        "image_id": variant_data.get("image_id",0),
                        "image_src":image_src
                    }
                )

        if products.has_next_page():
            time.sleep(2)
            products = products.next_page()
            continue

        break

    shopify.ShopifyResource.clear_session()


@shared_task
def backup_db():
    """
    A routine to take sql backup
    """
    my_env = os.environ.copy()
    
    my_env["PGPASSWORD"] = config('DB_PASS')

    # Define the pg_dump command as a list of strings
    pg_dump_command = [
        'pg_dump',              # The pg_dump command
        # Hostname (change to your PostgreSQL server's hostname)
        '-h', 'localhost',
        '-U', 'postgres',       # Username (change to your PostgreSQL username)
        # Database name (change to your PostgreSQL database name)
        '-d', 'thelonelykids',
    ]

    # Define the gzip command as a list of strings
    gzip_command = [
        'gzip',             # The gzip command
    ]

    try:
        # Run pg_dump and gzip as a pipeline
        pg_dump_process = subprocess.Popen(
            pg_dump_command, stdout=subprocess.PIPE, env=my_env)
        gzip_process = subprocess.Popen(
            gzip_command, stdin=pg_dump_process.stdout, stdout=subprocess.PIPE)

        # Wait for the gzip process to complete
        pg_dump_process.stdout.close()
        gzip_output = gzip_process.communicate()[0]

        # Write the gzipped output to a file (e.g., backup.sql.gz)
        backup_name = f'{timezone.now().strftime("Backup-%Y-%m-%d-%H-%M-%S")}.sql.gz'
        with open(f"{os.path.join(settings.MEDIA_ROOT, backup_name)}", 'wb') as backup_file:
            backup_file.write(gzip_output)

        backup_obj = Backup.objects.first()
        if not backup_obj:
            backup_obj = Backup.objects.create()

        if os.path.isfile(backup_obj.file.path):
            os.remove(backup_obj.file.path)
            
        backup_obj.timestamp = timezone.now()
        backup_obj.file.name = backup_name
        backup_obj.save()
        
        print("Database backup created and gzipped successfully.")
    except subprocess.CalledProcessError as e:
        print("Error running pg_dump or gzip:", e)
    except Exception as e:
        print("An unexpected error occurred:", e)
