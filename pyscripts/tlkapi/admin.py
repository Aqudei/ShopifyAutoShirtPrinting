from django.contrib import admin
from django.db.models import F
from .models import (Log, OrderInfo, LineItem, Bin,PrintRequest)
from .tasks import archive_bin_task
# Register your models here.


@admin.action(description="Empty selected bins")
def clear_bins(modeladmin, request, queryset):
    """
    docstring
    """
    for bin in queryset:
        archive_bin_task.delay(bin.Number)


@admin.register(Bin)
class BinAdmin(admin.ModelAdmin):
    list_display = ['Number', 'Active']

    actions = [clear_bins]


@admin.register(OrderInfo)
class OrderInfoAdmin(admin.ModelAdmin):
    list_display = ['OrderId', 'Bin', 'AllItemsPrinted', 'OrderNumber']
    list_filter = ['AllItemsPrinted']
    search_fields = ['OrderNumber']


@admin.register(LineItem)
class LineItemAdmin(admin.ModelAdmin):
    list_display = ['OrderNumber', 'Name', 'DateModified',
                    'Status', 'Quantity', 'PrintedQuantity','Shipping']
    search_fields = ['OrderNumber', 'Name']
    list_filter = ['Status','Shipping']

    readonly_fields = ['DateModified']


@admin.register(Log)
class LogAdmin(admin.ModelAdmin):
    list_display = ['ChangeDate', 'ChangeStatus', 'LineItem', 'order_number']
    search_fields = [
        'LineItem__OrderNumber',
        'LineItem__VariantTitle',
        'LineItem__Sku',
        'ChangeStatus'
    ]

    def order_number(self, obj):
        return obj.LineItem.OrderNumber

@admin.action(description="Mark Not Done")
def mark_not_done(modeladmin, request, queryset):
    """
    docstring
    """
  
    queryset.update(Done=False)
        
@admin.action(description="Toggle 'Done'")
def toggle_done(modeladmin, request, queryset):
    """
    docstring
    """
    done_value = ~F('Done') 
    queryset.update(Done=done_value)

@admin.register(PrintRequest)
class PrintRequestAdmin(admin.ModelAdmin):
    list_display = ['LineItem','Done','Timestamp','Sku', 'Variant']
    list_filter = ['Done']
    search_fields = [
        'LineItem__OrderNumber',
        'LineItem__VariantTitle',
        'LineItem__Sku'
    ]
    actions = [mark_not_done, toggle_done]

    def Sku(self,obj):
        """
        docstring
        """
        return obj.LineItem.Sku
    
