from django.db import models
from django.urls import reverse
from django.utils.translation import gettext_lazy as _

# Create your models here.


class Hook(models.Model):

    SOURCES = (('Shopify', 'Shopify'), ('ShipStation','ShipStation'))

    triggered_at = models.DateTimeField(
        _("Triggered At"), auto_now=False, auto_now_add=True)
    event = models.CharField(_("Event"), max_length=50, null=True, blank=True)
    body = models.JSONField(_("Body"), null=True, blank=True)
    headers = models.JSONField(_("Headers"), null=True, blank=True)
    processed = models.BooleanField(
        _("Processed"), default=False, null=True, blank=True)
    source = models.CharField(_("Source"), max_length=20, choices=SOURCES, default='Shopify')

    class Meta:
        verbose_name = _("hook")
        verbose_name_plural = _("hooks")

    def __str__(self):
        return self.event

    def get_absolute_url(self):
        return reverse("hook_detail", kwargs={"pk": self.pk})
