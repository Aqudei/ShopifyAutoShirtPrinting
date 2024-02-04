from django.shortcuts import render
from rest_framework import (
    generics, 
    mixins, 
    views, 
    response, 
    status,
    viewsets
)
from rest_framework.decorators import action

from tools.models import Variant, Product
from tools.serializers import (
    VariantSerializer,
    ProductSerializer
)
from rest_framework import filters, pagination
import json

# Create your views here.
class VariansViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = Variant.objects.all()
    serializer_class = VariantSerializer
    filter_backends = [filters.SearchFilter]
    search_fields = ['sku', 'product__handle', 'title']
    pagination_class = pagination.LimitOffsetPagination

class FindVariantAPI(generics.RetrieveAPIView):
    """
    docstring
    """
    serializer_class = VariantSerializer
    
    def get_object(self):

        variant_id = self.kwargs.get('variant_id')
        return Variant.objects.get(shopify_id=variant_id)

    
# Create your views here.
class ProductsViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = Product.objects.all()
    serializer_class = ProductSerializer
    filter_backends = [filters.SearchFilter]
    search_fields = ['handle','title']
    pagination_class = pagination.LimitOffsetPagination

    @action(detail=True)
    def list_variants(self,request, pk=None):
        """
        docstring
        """
        variants = Variant.objects.filter(product__pk=pk)
        serializer = VariantSerializer(variants, many=True)
        return response.Response(serializer.data)
