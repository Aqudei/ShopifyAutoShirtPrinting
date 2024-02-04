from django.shortcuts import render
from rest_framework import (views, generics, permissions, authentication)
from .models import Hook
from .serializers import HookSerializer
import hmac
import hashlib
import base64
from django.conf import settings


# class ShipStationWebhookHandlerView(generics.CreateAPIView):
#     queryset = Hook.objects.all()
#     serializer_class = HookSerializer
#     permission_classes = [permissions.AllowAny]

#     def perform_create(self, serializer):
#         return serializer.save(
#             headers=self.request.headers, body=self.request.data, source='ShipStation')


class ShopifyWebhookHandlerView(generics.CreateAPIView):
    """
    docstring
    """
    queryset = Hook.objects.all()
    serializer_class = HookSerializer
    permission_classes = [permissions.AllowAny]

    def perform_create(self, serializer):
        headers = {}
        for header in self.request.headers:
            if header.startswith("X-Shopify"):
                headers[header] = self.request.headers[header]
        event = self.request.headers.get("X-Shopify-Topic")
        triggered_at = self.request.headers.get("X-Shopify-Triggered-At")
        return serializer.save(
            headers=headers, event=event, body=self.request.data, triggered_at=triggered_at, source='Shopify')
