# Generated by Django 4.2.3 on 2023-10-01 13:21

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('tlkapi', '0020_printrequest'),
    ]

    operations = [
        migrations.AlterField(
            model_name='printrequest',
            name='Done',
            field=models.BooleanField(default=False, verbose_name='Done'),
        ),
    ]
