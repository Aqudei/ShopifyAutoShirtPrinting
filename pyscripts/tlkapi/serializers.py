from rest_framework import serializers
from .models import LineItem, Log, OrderInfo, Bin


class ReadLineItemSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    BinNumber = serializers.IntegerField(required=False)
    class Meta:
        model = LineItem
        fields = '__all__'
        read_only_fields = ['BinNumber']

class WriteLineItemSerializer(serializers.ModelSerializer):
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
    LineItems = ReadLineItemSerializer(many=True)
    class Meta:
        model = OrderInfo
        fields = '__all__'

class BinSerializer(serializers.Serializer):
    OrderNumber = serializers.CharField()
    BinNumber = serializers.IntegerField()
    LineItems = ReadLineItemSerializer(many=True)
    Notes = serializers.CharField(required=False)