from django.shortcuts import render
from rest_framework import views, generics
from .models import Hook
from .serializers import HookSerializer
# Create your views here.


class WebhookHandlerView(generics.CreateAPIView):
    """
    docstring
    """
    queryset = Hook.objects.all()
    serializer_class = HookSerializer


    def perform_create(self, serializer):
        headers = {}
        for header in self.request.headers:
            if header.startswith("X-Shopify"):
                headers[header] = self.request.headers[header]

        return serializer.save(headers=headers)