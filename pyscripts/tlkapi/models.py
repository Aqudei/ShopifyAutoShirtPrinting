# This is an auto-generated Django model module.
# You'll have to do the following manually to clean this up:
#   * Rearrange models' order
#   * Make sure each model has one field with primary_key=True
#   * Make sure each ForeignKey and OneToOneField has `on_delete` set to the desired behavior
#   * Remove `managed = False` lines if you wish to allow Django to create, modify, and delete the table
# Feel free to rename the models, but don't rename db_table values or field names.
from django.db import models
from django.utils.translation import gettext_lazy as _
from django.urls import reverse


class Bin(models.Model):
    Number = models.IntegerField(_("Bin Number"), unique=True)
    Active = models.BooleanField(_("Active"), default=False)
    Notes = models.TextField(_("Notes"), blank=True, null=True)

    class Meta:
        verbose_name = _("Bin")
        verbose_name_plural = _("Bins")
        ordering = ['Number']

    def __str__(self):
        return f"{self.Number}"

    def get_absolute_url(self):
        return reverse("Bin_detail", kwargs={"pk": self.pk})


class Log(models.Model):
    # Field name made lowercase.
    Id = models.AutoField(db_column='Id', primary_key=True)
    # Field name made lowercase.
    ChangeDate = models.DateTimeField(
        db_column='ChangeDate', auto_now_add=True, blank=True)
    # Field name made lowercase.
    ChangeStatus = models.CharField(
        db_column='ChangeStatus', blank=True, null=True, max_length=50)
    # Field name made lowercase,.
    LineItem = models.ForeignKey(
        'LineItem', models.SET_NULL, db_column='MyLineItemId', related_name='Logs', null=True)

    def __str__(self):
        return f"{self.ChangeDate} - {self.LineItem} - {self.ChangeStatus}"

    class Meta:
        db_table = 'Logs'


class LineItem(models.Model):
    # Field name made lowercase.
    Id = models.AutoField(db_column='Id', primary_key=True)
    # Field name made lowercase.
    OrderNumber = models.CharField(
        db_column='OrderNumber', max_length=50)
    # Field name made lowercase.
    Sku = models.CharField(db_column='Sku', blank=True,
                           null=True, max_length=100)
    # Field name made lowercase.
    Name = models.CharField(db_column='Name', blank=True,
                            null=True, max_length=500)
    # Field name made lowercase.
    VariantId = models.BigIntegerField(
        db_column='VariantId', blank=True, null=True)
    # Field name made lowercase.
    VariantTitle = models.CharField(
        db_column='VariantTitle', blank=True, null=True, max_length=500)
    # Field name made lowercase.
    LineItemId = models.BigIntegerField(
        db_column='LineItemId', blank=True, null=True, unique=True)
    # Field name made lowercase.
    Quantity = models.IntegerField(
        db_column='Quantity', blank=True, null=True, default=0)
    # Field name made lowercase.
    FulfillmentStatus = models.CharField(
        db_column='FulfillmentStatus', blank=True, null=True, max_length=64)
    # Field name made lowercase.
    FinancialStatus = models.CharField(
        db_column='FinancialStatus', blank=True, null=True, max_length=64)
    # Field name made lowercase.
    Customer = models.CharField(
        db_column='Customer', blank=True, null=True, max_length=100)
    # Field name made lowercase.
    CustomerEmail = models.EmailField(
        db_column='CustomerEmail', blank=True, null=True)
    # Field name made lowercase.
    DateModified = models.DateTimeField(
        db_column='DateModified', blank=True, null=True, auto_now=True)
    # Field name made lowercase.
    ProductImage = models.CharField(
        db_column='ProductImage', blank=True, null=True, max_length=500)
    # Field name made lowercase.
    Notes = models.TextField(db_column='Notes', blank=True, null=True)
    # Field name made lowercase.
    OrderId = models.BigIntegerField(
        db_column='OrderId', blank=True, null=True)
    # Field name made lowercase.
    PrintedQuantity = models.IntegerField(
        db_column='PrintedQuantity', default=0)
    # Field name made lowercase.
    Status = models.CharField(
        db_column='Status', blank=True, null=True, max_length=32, default="Pending")
    # Field name made lowercase.
    Shipping = models.CharField(
        db_column='Shipping', blank=True, null=True, max_length=128)
    Order = models.ForeignKey(
        "tlkapi.OrderInfo", verbose_name=_("Order"), on_delete=models.SET_NULL, null=True, blank=True, related_name='LineItems')

    def __str__(self):
        return self.Name

    class Meta:
        db_table = 'MyLineItems'
        ordering = ['-OrderNumber']


class OrderInfoManager(models.Manager):
    """
    docstring
    """

    def __parse_int(self, intstr):
        """
        docstring
        """
        try:
            return int(intstr)
        except:
            return 0

    def new_order_number(self):
        order = self.order_by("OrderNumber").first()
    
        if order and self.__parse_int(order.OrderNumber) <= 0:
            return self.__parse_int(order.OrderNumber) - 1
        else:
            return -1


class OrderInfo(models.Model):
    # Field name made lowercase.
    Id = models.AutoField(db_column='Id', primary_key=True)
    # Field name made lowercase.
    Bin = models.OneToOneField("tlkapi.Bin", verbose_name=_(
        "Bin"), on_delete=models.SET_NULL, null=True, blank=True, related_name='Order')
    # Field name made lowercase.
    OrderId = models.BigIntegerField(
        db_column='OrderId', null=True, blank=True)
    OrderNumber = models.CharField(
        _("OrderNumber"), max_length=50, default="0")
    # Field name made lowercase.
    # Field name made lowercase.
    LabelPrinted = models.BooleanField(db_column='LabelPrinted', default=False)
    # Field name made lowercase.
    LabelData = models.TextField(db_column='LabelData', blank=True, null=True)
    # Field name made lowercase.
    TrackingNumber = models.CharField(
        db_column='TrackingNumber', blank=True, null=True, max_length=500)
    # Field name made lowercase.
    InsuranceCost = models.FloatField(db_column='InsuranceCost', default=0.0)
    # Field name made lowercase.
    ShipmentCost = models.FloatField(db_column='ShipmentCost', default=0.0)
    # Field name made lowercase.
    ShipmentId = models.IntegerField(db_column='ShipmentId', default=0)
    AllItemsPrinted = models.BooleanField(_("All Printed"), default=False)
    objects = OrderInfoManager()

    def __str__(self) -> str:
        return f"{self.OrderId} - {self.Bin.Number if self.Bin else 0}"

    class Meta:
        db_table = 'OrderInfoes'
