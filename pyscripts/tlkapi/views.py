from django.shortcuts import render
from rest_framework import views, viewsets,generics,permissions,authentication
from .models import LineItem, Log
from .serializers import LineItemSerializer, LogSerializer

# Create your views here.

class LineItemViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = LineItem.objects.exclude(Status='Archived')
    serializer_class = LineItemSerializer

class LogView(generics.ListCreateAPIView):
    queryset = Log.objects.all()
    serializer_class = LogSerializer

    def list(self, request, *args, **kwargs):
        queryset = self.get_queryset()
        queryset.filter(LineItem=self.kwargs['LineItem'])
