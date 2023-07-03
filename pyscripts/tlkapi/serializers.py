from rest_framework import serializers
from .models import LineItem,Log

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

