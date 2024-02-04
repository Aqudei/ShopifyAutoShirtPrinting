from django.shortcuts import render
from rest_framework import views, response
from django.conf import settings
from accounts.models import UserProfile
# Create your views here.


class SessionInfoView(views.APIView):
    """
    Retrive session info
    """
    def get(self, request):
        data=dict()
        data['logging_email'] = settings.LOGGING_EMAIL
        data['logging_password'] = settings.LOGGING_PASSWORD
        data['broadcast_exchange'] = settings.BROADCAST_EXCHANGE
        data['broadcast_username'] = settings.BROADCAST_USERNAME
        data['broadcast_password'] = settings.BROADCAST_PASSWORD
        data['broadcast_host'] = settings.BROADCAST_HOST
        data['shipstation_username'] = UserProfile.server_options.shipstation_username
        data['shipstation_password'] = UserProfile.server_options.shipstation_password

        return response.Response(data=data)