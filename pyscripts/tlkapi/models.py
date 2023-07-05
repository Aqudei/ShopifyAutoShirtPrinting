# This is an auto-generated Django model module.
# You'll have to do the following manually to clean this up:
#   * Rearrange models' order
#   * Make sure each model has one field with primary_key=True
#   * Make sure each ForeignKey and OneToOneField has `on_delete` set to the desired behavior
#   * Remove `managed = False` lines if you wish to allow Django to create, modify, and delete the table
# Feel free to rename the models, but don't rename db_table values or field names.
from django.db import models
from django.utils.translation import gettext_lazy as _


class Log(models.Model):
    # Field name made lowercase.
    Id = models.AutoField(db_column='Id', primary_key=True)
    # Field name made lowercase.
    ChangeDate = models.DateTimeField(
        db_column='ChangeDate', auto_now_add=True, blank=True)
    # Field name made lowercase.
    ChangeStatus = models.TextField(
        db_column='ChangeStatus', blank=True, null=True)
    # Field name made lowercase,.
    LineItem = models.ForeignKey(
        'LineItem', models.SET_NULL, db_column='MyLineItemId', related_name='Logs', null=True)

    class Meta:
        db_table = 'Logs'


class LineItem(models.Model):
    # Field name made lowercase.
    Id = models.AutoField(db_column='Id', primary_key=True)
    # Field name made lowercase.
    OrderNumber = models.TextField(
        db_column='OrderNumber', blank=True, null=True)
    # Field name made lowercase.
    Sku = models.TextField(db_column='Sku', blank=True, null=True)
    # Field name made lowercase.
    Name = models.TextField(db_column='Name', blank=True, null=True)
    # Field name made lowercase.
    VariantId = models.BigIntegerField(
        db_column='VariantId', blank=True, null=True)
    # Field name made lowercase.
    VariantTitle = models.TextField(
        db_column='VariantTitle', blank=True, null=True)
    # Field name made lowercase.
    LineItemId = models.BigIntegerField(
        db_column='LineItemId', blank=True, null=True)
    # Field name made lowercase.
    Quantity = models.IntegerField(db_column='Quantity', blank=True, null=True)
    # Field name made lowercase.
    FulfillmentStatus = models.TextField(
        db_column='FulfillmentStatus', blank=True, null=True)
    # Field name made lowercase.
    FinancialStatus = models.TextField(
        db_column='FinancialStatus', blank=True, null=True)
    # Field name made lowercase.
    Customer = models.TextField(db_column='Customer', blank=True, null=True)
    # Field name made lowercase.
    CustomerEmail = models.TextField(
        db_column='CustomerEmail', blank=True, null=True)
    # Field name made lowercase.
    DateModified = models.DateTimeField(
        db_column='DateModified', blank=True, null=True, auto_now=True)
    # Field name made lowercase.
    ProductImage = models.TextField(
        db_column='ProductImage', blank=True, null=True)
    # Field name made lowercase.
    Notes = models.TextField(db_column='Notes', blank=True, null=True)
    # Field name made lowercase.
    OrderId = models.BigIntegerField(
        db_column='OrderId', blank=True, null=True)
    # Field name made lowercase.
    PrintedQuantity = models.IntegerField(db_column='PrintedQuantity')
    # Field name made lowercase.
    BinNumber = models.IntegerField(db_column='BinNumber')
    # Field name made lowercase.
    Status = models.TextField(db_column='Status', blank=True, null=True)
    # Field name made lowercase.
    Shipping = models.TextField(db_column='Shipping', blank=True, null=True)
    Order = models.ForeignKey(
        "tlkapi.OrderInfo", verbose_name=_("Order"), on_delete=models.SET_NULL, null=True, related_name='LineItems')

    class Meta:
        db_table = 'MyLineItems'
        ordering = ['-OrderNumber']


class OrderInfo(models.Model):
    # Field name made lowercase.
    Id = models.AutoField(db_column='Id', primary_key=True)
    # Field name made lowercase.
    BinNumber = models.IntegerField(db_column='BinNumber')
    # Field name made lowercase.
    OrderId = models.BigIntegerField(db_column='OrderId')
    # Field name made lowercase.
    Active = models.BooleanField(db_column='Active')
    # Field name made lowercase.
    LabelPrinted = models.BooleanField(db_column='LabelPrinted')
    # Field name made lowercase.
    LabelData = models.TextField(db_column='LabelData', blank=True, null=True)
    # Field name made lowercase.
    TrackingNumber = models.TextField(
        db_column='TrackingNumber', blank=True, null=True)
    # Field name made lowercase.
    InsuranceCost = models.FloatField(db_column='InsuranceCost')
    # Field name made lowercase.
    ShipmentCost = models.FloatField(db_column='ShipmentCost')
    # Field name made lowercase.
    ShipmentId = models.IntegerField(db_column='ShipmentId')

    class Meta:
        db_table = 'OrderInfoes'
