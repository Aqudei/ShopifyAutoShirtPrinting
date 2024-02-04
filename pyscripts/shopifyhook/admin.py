from django.contrib import admin
from django.db.models import Count, F, Value
from .models import Hook
# Register your models here.

@admin.action(description="Toggle 'processed' value for selected event/s")
def toggle_processed(modeladmin, request, queryset):
    queryset.update(processed = ~F('processed'))

@admin.register(Hook)
class HookAdmin(admin.ModelAdmin):
    list_display = ['triggered_at', 'event', 'headers', 'processed','source', 'body']
    list_filter = ['event','processed','source']
    search_fields = ['body__name']
    actions = [toggle_processed]
