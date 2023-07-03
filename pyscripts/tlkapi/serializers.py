from rest_framework import serializers
from .models import LineItem, Log, OrderInfoViewSet


class LineItemSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    class Meta:
        model = LineItem
        fields = '__all__'


class LogSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    class Meta:
        model = Log
        fields = '__all__'


class OrderInfoSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    class Meta:
        model = OrderInfoViewSet
        fields = '__all__'
