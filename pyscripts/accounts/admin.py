from django.contrib import admin
from django.contrib.auth.admin import UserAdmin as BaseUserAdmin
from django.contrib.auth.models import User

from accounts.models import UserProfile
# Register your models here.
class UserProfileInline(admin.StackedInline):
    model = UserProfile
    can_delete=False
    verbose_name_plural = 'User Profile'
    
class UserAdmin(BaseUserAdmin):
    """
    docstring
    """
    inlines = (UserProfileInline,)
    
# Re-register UserAdmin
admin.site.unregister(User)
admin.site.register(User, UserAdmin)