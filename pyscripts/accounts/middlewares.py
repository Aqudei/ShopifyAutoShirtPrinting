import pytz
from django.utils import timezone

from accounts.models import UserProfile

class TimezoneMiddleware:
    def __init__(self, get_response):
        self.get_response = get_response

    def __call__(self, request):
        if request.user.is_authenticated:
            user_profile, created = UserProfile.objects.get_or_create(user=request.user)
            timezone.activate(user_profile.timezone)
        else:
            timezone.deactivate()

        response = self.get_response(request)
        return response
