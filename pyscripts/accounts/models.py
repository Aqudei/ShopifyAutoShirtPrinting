from django.db import models
from django.conf import settings
from django.urls import reverse
from django.utils.translation import gettext_lazy as _
import pytz
import dbsettings

# Create your models here.

class ServerOptions(dbsettings.Group):
    shipstation_username = dbsettings.StringValue()
    shipstation_password = dbsettings.StringValue()
    

class UserProfile(models.Model):
    TIMEZONES = [(tz, tz) for tz in pytz.all_timezones]
    user = models.OneToOneField(settings.AUTH_USER_MODEL, on_delete=models.CASCADE)
    timezone = models.CharField(
        max_length=50, choices=TIMEZONES, default="UTC")
    server_options = ServerOptions()
    
    class Meta:
        verbose_name = _("user profile")
        verbose_name_plural = _("user profiles")

    def __str__(self):
        return f"{self.user}"

    def get_absolute_url(self):
        return reverse("userprofile_detail", kwargs={"pk": self.pk})

