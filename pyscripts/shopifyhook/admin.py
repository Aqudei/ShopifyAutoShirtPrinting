from django.contrib import admin
from .models import Hook
# Register your models here.


@admin.register(Hook)
class HookAdmin(admin.ModelAdmin):
    list_display = ['timestamp', 'event', 'headers', 'processed', 'body']
