from rest_framework import serializers
from .models import LineItem, Log, OrderInfo, Bin


class LineItemSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    BinNumber = serializers.IntegerField(required=False)
    class Meta:
        model = LineItem
        fields = '__all__'
        read_only_fields = ['BinNumber']

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
    LineItems = LineItemSerializer(many=True)
    class Meta:
        model = OrderInfo
        fields = '__all__'

class BinSerializer(serializers.Serializer):
    BinNumber = serializers.IntegerField()
    LineItems = LineItemSerializer(many=True)

    class Meta:
        """
        docstring
        """
        model = Bin
        fields = '__all__'