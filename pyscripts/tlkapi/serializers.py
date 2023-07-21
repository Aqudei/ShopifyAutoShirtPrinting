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

class ReadBinSerializer(serializers.ModelSerializer):
    OrderNumber = serializers.CharField(source='Order.OrderNumber')
    BinNumber = serializers.IntegerField(source='Number')
    LineItems = ReadLineItemSerializer(source='Order.LineItems', many=True)

    class Meta:
        """
        docstring
        """
        model = Bin
        fields = "__all__"


class BinSerializer(serializers.ModelSerializer):
    class Meta:
        """
        docstring
        """
        model = Bin
        fields = "__all__"