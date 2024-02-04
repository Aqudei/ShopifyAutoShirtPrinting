import json
from rest_framework import serializers
from .models import (
    LineItem,
    Log,
    OrderInfo,
    Bin,
    PrintRequest
)
from tools.serializers import VariantSerializer
from django.conf import settings

class ReadLineItemSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    BinNumber = serializers.IntegerField(required=False)
    OriginalBinNumber = serializers.IntegerField(required=False)
    
    class Meta:
        model = LineItem
        fields = '__all__'
        read_only_fields = ['BinNumber', 'OriginalBinNumber']

    def to_representation(self, instance):
        representation = super().to_representation(instance)

        for_pickup = False
        
        if not instance.Shipping:
            representation['ForPickup'] = for_pickup
            return representation
        
        for for_pickup_shipping_line in settings.SHIPPING_LINES_FOR_PICKUP:
            if for_pickup_shipping_line in instance.Shipping:
                for_pickup = True
                break
           
        representation['ForPickup'] = for_pickup

        return representation
    
class WriteLineItemSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    class Meta:
        model = LineItem
        fields = '__all__'
        read_only_fields = ['DateModified']


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
    # LineItems = ReadLineItemSerializer(source='Order.LineItems', many=True)

    class Meta:
        """
        docstring
        """
        model = Bin
        fields = "__all__"

    def to_representation(self, instance):
        representation = super().to_representation(instance)
        representation['LineItems'] = ReadLineItemSerializer(
            instance.Order.LineItems.exclude(Status='Archived'), many=True).data

        return representation


class BinSerializer(serializers.ModelSerializer):
    class Meta:
        """
        docstring
        """
        model = Bin
        fields = "__all__"


class PrintRequestSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    Variant = VariantSerializer()
    class Meta:
        """
        docstring
        """
        model = PrintRequest
        fields = '__all__'
        read_only_fields = ('Variant',)


    def to_representation(self, instance):
        data = super(PrintRequestSerializer,self).to_representation(instance)
        line_item_id = data.pop("LineItem")

        line_item_serializer = ReadLineItemSerializer(LineItem.objects.get(Id=line_item_id))
        data['LineItem'] = line_item_serializer.data

        return data


class IdArraySerializer(serializers.Serializer):
    """
    docstring
    """
    ids = serializers.ListField()