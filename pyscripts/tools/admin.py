from django.contrib import admin
from tools.models import (
    Product,
    Variant,
    Backup,
    ProductType
)
from django.db.models import F

@admin.action(description="Toggle Has BackPrint value")
def toggle_backprint(modeladmin, request, queryset):
    queryset.update(has_backprint=~F('has_backprint'))

@admin.register(Backup)
class BackupAdmin(admin.ModelAdmin):
    list_display = ['timestamp', 'file', 'size']

    def size(self, obj):
        """
        docstring
        """
        return obj.file.size


class VariantInline(admin.StackedInline):
    model = Variant
    extra = 1
    
    

@admin.register(Variant)
class VariantAdmin(admin.ModelAdmin):
    list_display = [
        'product', 
        'title', 
        'shopify_id', 
        'sku', 
        'option1',
        'option2',
        'option3',
        'product_type',
        'updated_at',
        'created_at'
    ]
    
    search_fields = ['product__handle',
                     'product__product_type', 'product__title','shopify_id']
    list_filter = ['option1','option2','option3','product__product_type']
    readonly_fields = ('updated_at','created_at')
    def product_type(self,obj):
        """
        docstring
        """
        return obj.product.product_type

@admin.register(Product)
class ProductAdmin(admin.ModelAdmin):
    list_display = ['handle', 
                    'shopify_id',
                    'product_type', 
                    'is_virtual', 
                    'title',
                    'has_backprint',
                    'updated_at',
                    'created_at'
                ]
    
    list_filter = ['product_type', 'sku_fixed','has_backprint']
    search_fields = ['handle', 'product_type', 'title']
    actions = (toggle_backprint,)
    inlines = (VariantInline,)
    readonly_fields = ('updated_at','created_at')
    
@admin.register(ProductType)
class ProductTypeAdmin(admin.ModelAdmin):
    list_display = ['name', 'code']
