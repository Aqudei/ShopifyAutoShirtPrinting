from django.shortcuts import render
from rest_framework import (views, generics, permissions, authentication)
from .models import Hook
from .serializers import HookSerializer
import hmac
import hashlib
import base64
from django.conf import settings


class WebhookHandlerView(generics.CreateAPIView):
    """
    docstring
    """
    queryset = Hook.objects.all()
    serializer_class = HookSerializer
    permission_classes = permissions.AllowAny

    def perform_create(self, serializer):
        headers = {}
        for header in self.request.headers:
            if header.startswith("X-Shopify"):
                headers[header] = self.request.headers[header]

        return serializer.save(headers=headers)