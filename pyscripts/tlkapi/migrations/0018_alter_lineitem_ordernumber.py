# Generated by Django 4.2.3 on 2023-07-25 09:38

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('tlkapi', '0017_alter_orderinfo_ordernumber'),
    ]

    operations = [
        migrations.AlterField(
            model_name='lineitem',
            name='OrderNumber',
            field=models.CharField(db_column='OrderNumber', default='0', max_length=50),
            preserve_default=False,
        ),
    ]