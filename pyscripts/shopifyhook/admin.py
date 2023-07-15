from django.contrib import admin
from .models import Hook
# Register your models here.


@admin.register(Hook)
class HookAdmin(admin.ModelAdmin):
    list_display = ['triggered_at', 'event', 'headers', 'processed','source', 'body']
    list_filter = ['event','processed','source']

