from django.contrib import admin
from .models import Log, OrderInfo, LineItem
# Register your models here.


@admin.register(Log)
class LogAdmin(admin.ModelAdmin):
    list_display = ['ChangeDate','ChangeStatus','LineItem']