from rest_framework import serializers
from .models import LineItem, Log, OrderInfo


class LineItemSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    class Meta:
        model = LineItem
        fields = '__all__'

class BinSerializer(serializers.Serializer):
    BinNumber = serializers.IntegerField()
    OrderNumber = serializers.CharField()
    Items = LineItemSerializer(many=True)

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
        model = OrderInfo
        fields = '__all__'
