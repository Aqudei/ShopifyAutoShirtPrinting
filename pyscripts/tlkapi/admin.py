from django.contrib import admin
from .models import Log, OrderInfo, LineItem, Bin
# Register your models here.

@admin.register(Bin)
class BinAdmin(admin.ModelAdmin):
    list_display = ['Number','Active']

@admin.register(OrderInfo)
class OrderInfoAdmin(admin.ModelAdmin):
    list_display = ['OrderId', 'Bin']


@admin.register(LineItem)
class LineItemAdmin(admin.ModelAdmin):
    list_display = ['OrderNumber', 'Name', 'Status']
    search_fields = ['OrderNumber', 'Name']
    list_filter = ['Status']


@admin.register(Log)
class LogAdmin(admin.ModelAdmin):
    list_display = ['ChangeDate', 'ChangeStatus', 'LineItem']
