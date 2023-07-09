from django.shortcuts import render
from django.db.models import Sum, Count
from django_filters import rest_framework as filters
from rest_framework.exceptions import APIException
from django.db.models import Count, F, Value
from tlkapi import tasks

from rest_framework import (
    views,
    viewsets,
    generics,
    permissions,
    authentication,
    decorators,
    response,
    status
)
from .models import (
    LineItem,
    Log,
    OrderInfo,
    Bin
)
from .serializers import (
    ReadLineItemSerializer,
    WriteLineItemSerializer,
    LogSerializer,
    OrderInfoSerializer,
    BinSerializer
)
from .tasks import reset_database_task

# Create your views here.

class LineItemViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    def get_queryset(self):
        queryset = LineItem.objects.exclude(Status='Archived').annotate(BinNumber=F('Order__Bin__Number'))
        return queryset
    
    # queryset = LineItem.objects.exclude(Status='Archived')
    serializer_class = ReadLineItemSerializer

    def get_serializer_class(self):
        if self.request.method == 'GET':
            return self.serializer_class
        else:
            return WriteLineItemSerializer
        
    def perform_create(self, serializer):    
        instance = serializer.save()
        tasks.broadcast_added.delay([instance.Id])
        return instance
    
    def perform_update(self, serializer):
        instance = serializer.save()
        tasks.broadcast_updated.delay([instance.Id])
        return instance
    
    filterset_fields =  ["Id",'LineItemId', "OrderId"]

class ListLineItemsView(views.APIView):
    """
    docstring
    """
    def get(self,request):
        """
        docstring
        """
        ids = [int(id) for id in self.request.query_params.getlist('Id')]
        queryset = LineItem.objects.filter(Id__in=ids)
        serializer = ReadLineItemSerializer(queryset,many=True)
        
        return response.Response(serializer.data)


class OrderInfoViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = OrderInfo.objects.all()

    serializer_class = OrderInfoSerializer
    filterset_fields = ["OrderId"]


class LogAPIView(generics.ListCreateAPIView):
    queryset = Log.objects.all()
    serializer_class = LogSerializer
    filterset_fields = ['LineItem']

class DestroyBinView(views.APIView):
    """
    docstring
    """
    def delete(self,request, pk=None):
        """
        docstring
        """
        bin = Bin.objects.get(pk=pk)
        order = OrderInfo.objects.get(Bin=bin)
        bin.Active = False
        bin.save()

        order.Bin = None
        order.save()

class ProcessItemView(views.APIView):
    """
    docstring
    """
    def post(self,request, pk=None):
        """
        docstring
        """
        bin = None
        line_item = LineItem.objects.get(pk=pk)

        line_items_aggregate = line_item.Order.LineItems.aggregate(
            total_quantity= Sum('Quantity'),
            total_printed = Sum('PrintedQuantity')
        )

        all_items_printed = line_items_aggregate['total_printed'] >= line_items_aggregate['total_quantity'] 
        if all_items_printed:
            serializer = ReadLineItemSerializer(line_item)
            data = {
                "LineItem": serializer.data,
                "AllItemsPrinted" : all_items_printed
            }
            return response.Response(serializer.data)
        
        order_info = line_item.Order

        # Case 1, only one item, no need to assign Bin
        if line_items_aggregate['total_quantity'] <= 1:
            bin = Bin.objects.get(Number=0)

        else:
            bin = Bin.objects.exclude(Number=0).filter(Active=False).first()
            if not bin:
                raise APIException(detail="No available Bin")
            bin.Active = True
            bin.save()
        
        order_info.Bin = bin
        order_info.save()
        
        line_item.Status = "Processed"
        line_item.PrintedQuantity += 1
        line_item.save()
        
        Log.objects.create(
            ChangeStatus = "Processed",
            LineItem = line_item
        )
        
        line_items_aggregate = line_item.Order.LineItems.aggregate(
            total_quantity= Sum('Quantity'),
            total_printed = Sum('PrintedQuantity')
        )

        all_items_printed = line_items_aggregate['total_printed'] >= line_items_aggregate['total_quantity'] 
        
        # tasks.broadcast_change([line_item.Id]) 
        line_item.refresh_from_db()
        serializer = ReadLineItemSerializer(line_item)
        data = {
            "LineItem": serializer.data,
            "AllItemsPrinted" : all_items_printed
        }

        return response.Response(data)
    
class ResetDatabaseAPIView(views.APIView):
    """
    docstring
    """
    def post(self,request,*args, **kwargs):
        """
        docstring
        """
        reset_database_task.delay()
        return response.Response({"message","Database reset"})

class ListBinsView(views.APIView):
    """
    docstring
    """
    serializer_class = BinSerializer
    
    def get(self, request, *args, **kwargs):
        active_bins = Bin.objects.filter(Active=True)
        in_bin_orders = OrderInfo.objects.filter(Bin__in=active_bins)

        data = []

        for order in in_bin_orders:
            line_items = LineItem.objects.filter(Order=order)
            serializer = ReadLineItemSerializer(line_items, many=True)
            data.append({
                "OrderNumber" : order.OrderNumber,
                "BinNumber": order.Bin.Number,
                "LineItems": serializer.data
            })
        
        return response.Response(data)
        
        
        
