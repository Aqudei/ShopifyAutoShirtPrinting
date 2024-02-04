from rest_framework import serializers
from tools import models


class ProductSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    class Meta:
        model = models.Product
        fields = '__all__'


class VariantSerializer(serializers.ModelSerializer):
    """
    docstring
    """
    product = ProductSerializer()

    class Meta:
        model = models.Variant
        fields = '__all__'