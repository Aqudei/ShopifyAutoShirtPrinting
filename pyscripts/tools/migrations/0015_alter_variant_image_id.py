# Generated by Django 4.2.6 on 2024-01-15 14:37

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('tools', '0014_product_is_virtual'),
    ]

    operations = [
        migrations.AlterField(
            model_name='variant',
            name='image_id',
            field=models.PositiveBigIntegerField(blank=True, null=True, verbose_name='Image Id'),
        ),
    ]