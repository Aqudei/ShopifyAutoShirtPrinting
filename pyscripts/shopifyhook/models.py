from django.db import models
from django.urls import reverse
from django.utils.translation import gettext_lazy as _

# Create your models here.


class Hook(models.Model):

    timestamp = models.DateTimeField(
        _("Timestamp"), auto_now=False, auto_now_add=True)
    event = models.CharField(_("Event"), max_length=50)
    body = models.JSONField(_("Body"), null=True, blank=True)
    headers = models.JSONField(_("Headers"), null=True, blank=True)
    processed = models.BooleanField(_("Processed"), default=False)

    class Meta:
        verbose_name = _("hook")
        verbose_name_plural = _("hooks")

    def __str__(self):
        return self.event

    def get_absolute_url(self):
        return reverse("hook_detail", kwargs={"pk": self.pk})
