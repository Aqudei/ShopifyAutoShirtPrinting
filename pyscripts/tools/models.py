from django.db import models
from django.urls import reverse
from django.utils.translation import gettext_lazy as _


class Variant(models.Model):

    product = models.ForeignKey("tools.Product", verbose_name=_(
        "Product"), on_delete=models.CASCADE)
    title = models.CharField(_("Title"), max_length=200)
    shopify_id = models.PositiveBigIntegerField(_("Variant Id"))
    option1 = models.CharField(
        _("Option 1"), max_length=100, null=True, blank=True)
    option2 = models.CharField(
        _("Option 2"), max_length=100, null=True, blank=True)
    option3 = models.CharField(
        _("Option 3"), max_length=100, null=True, blank=True)
    sku = models.CharField(_("Sku"), max_length=100, blank=True, null=True)
    new_sku = models.CharField(
        _("New Sku"), max_length=100, blank=True, null=True)
    image_id = models.PositiveBigIntegerField(_("Image Id"), null=True,blank=True)
    
    image_src = models.URLField(_("Image Source"), blank=True,null=True)
    updated_at = models.DateTimeField(_("Updated At"), auto_now=True, null=True, blank=True)
    created_at = models.DateTimeField(_("Created At"), auto_now_add=True, null=True, blank=True)
    
    class Meta:
        verbose_name = _("variant")
        verbose_name_plural = _("variants")

    def __str__(self):
        return f"{self.product} - {self.title}"

    def get_absolute_url(self):
        return reverse("variant_detail", kwargs={"pk": self.pk})


class Product(models.Model):

    handle = models.CharField(_("Handle"), max_length=200)
    title = models.CharField(_("Title"), max_length=250, null=True, blank=True)
    shopify_id = models.PositiveBigIntegerField("Product Id")
    product_type = models.CharField(_("Product Type"), max_length=50)
    sku_fixed = models.BooleanField(_("Sku Fixed"), default=False)
    has_backprint = models.BooleanField(_("Has Back Print"), default=False)
    updated_at = models.DateTimeField(_("Updated At"), auto_now=True, null=True, blank=True)
    created_at = models.DateTimeField(_("Created At"), auto_now_add=True, null=True, blank=True)
    
    is_virtual = models.BooleanField(_("Virtual Product"), default=False)
    
    class Meta:
        verbose_name = _("product")
        verbose_name_plural = _("products")

    def __str__(self):
        return self.handle

    def get_absolute_url(self):
        return reverse("product_detail", kwargs={"pk": self.pk})


class Backup(models.Model):

    timestamp = models.DateTimeField(_("Backup Date"), auto_now=True)
    file = models.FileField(_("File"), upload_to='backup', max_length=100)

    class Meta:
        verbose_name = _("backup")
        verbose_name_plural = _("backups")

    def __str__(self):
        return f'{self.timestamp}'

    def get_absolute_url(self):
        return reverse("backup_detail", kwargs={"pk": self.pk})


class ProductType(models.Model):

    name = models.CharField(_("Product Type"), max_length=100)
    code = models.CharField(
        _("Type Code"), max_length=50, null=True, blank=True)

    class Meta:
        verbose_name = _("Product Type")
        verbose_name_plural = _("Product Types")

    def __str__(self):
        return f"{self.name} ({self.code})"

    def get_absolute_url(self):
        return reverse("producttype_detail", kwargs={"pk": self.pk})
