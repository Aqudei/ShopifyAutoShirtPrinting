from django.shortcuts import render
from rest_framework import views, viewsets, generics, permissions, authentication, decorators
from .models import LineItem, Log, OrderInfoViewSet
from .serializers import LineItemSerializer, LogSerializer, OrderInfoSerializer

# Create your views here.


class LineItemViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = LineItem.objects.exclude(Status='Archived')
    serializer_class = LineItemSerializer
    filterset_fields = ['LineItemId', "OrderId"]


class OrderInfoViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = OrderInfoViewSet.objects.all()

    serializer_class = OrderInfoSerializer
    filterset_fields = ['BinNumber', "OrderId", "Active"]


class LogAPIView(generics.ListCreateAPIView):
    queryset = Log.objects.all()
    serializer_class = LogSerializer
    filterset_fields = ['MyLineItemId']
