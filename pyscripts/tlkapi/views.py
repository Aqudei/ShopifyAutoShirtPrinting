import json
import logging
from uuid import uuid4
from django.shortcuts import render
from django.db.models import Sum, Count
from django_filters import rest_framework as filters
from rest_framework.exceptions import APIException
from django.db.models import Count, F, Value
from tlkapi import tasks
from django.conf import settings

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
    BinSerializer,
    ReadLineItemSerializer,
    WriteLineItemSerializer,
    LogSerializer,
    OrderInfoSerializer,
    ReadBinSerializer
)
from .tasks import reset_database_task

logger = logging.getLogger(__name__)

class LineItemViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    filterset_fields = ["Id", 'LineItemId', "OrderId", "OrderNumber","Status"]
    serializer_class = ReadLineItemSerializer
    # queryset = LineItem.objects.exclude(Status='Archived')

    def get_queryset(self):
        if self.request.query_params.get("Status") == 'Archived':
            queryset = LineItem.objects.filter(Status='Archived').annotate(
                BinNumber=F('Order__Bin__Number'))
        else:
            queryset = LineItem.objects.exclude(Status='Archived').annotate(
                BinNumber=F('Order__Bin__Number'))
        return queryset

    def get_serializer_class(self):
        if self.request.method == 'GET':
            return self.serializer_class
        else:
            return WriteLineItemSerializer

    def perform_create(self, serializer):
        instance = serializer.save()

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_added.delay([instance.Id])
            tasks.populate_info.delay(instance.Id)

        return instance

    def perform_update(self, serializer):
        instance = serializer.save()

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_updated.delay([instance.Id])

        return instance




class ListLineItemsView(views.APIView):
    """
    docstring
    """

    def get(self, request):
        """
        docstring
        """
        ids = [int(id) for id in self.request.query_params.getlist('Id')]
        queryset = LineItem.objects.filter(Id__in=ids).annotate(
            BinNumber=F('Order__Bin__Number'))
        serializer = ReadLineItemSerializer(queryset, many=True)

        return response.Response(serializer.data)


class OrderInfoViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = OrderInfo.objects.all()
    serializer_class = OrderInfoSerializer
    filterset_fields = ["OrderId",'Id','OrderNumber']


class LogAPIView(generics.ListCreateAPIView):
    queryset = Log.objects.all()
    serializer_class = LogSerializer
    filterset_fields = ['LineItem']


class DestroyBinView(views.APIView):
    """
    docstring
    """

    def delete(self, request, BinNumber=None):
        """
        docstring
        """
        bin = Bin.objects.get(Number=BinNumber)
        bin.Active = False
        bin.save()

        order = OrderInfo.objects.get(Bin=bin)
        order.Bin = None
        order.save()

        if settings.BROADCAST_ENABLED:
            tasks.broadcast.delay([bin.Number],"bins.destroyed")
            
        return response.Response()

class ItemProcessingView(views.APIView):
    """
    docstring
    """

    def delete(self, request, pk=None):
        """
        docstring
        """
        line_item = LineItem.objects.get(Id=pk)
        order_info = line_item.Order

        if line_item.PrintedQuantity >= 1:
            line_item.PrintedQuantity = line_item.PrintedQuantity - 1

        if line_item.PrintedQuantity == 0:
            line_item.Status = "Pending"

        line_item.save()

        line_items_aggregate = order_info.LineItems.aggregate(
            total_quantity=Sum('Quantity'),
            total_printed=Sum('PrintedQuantity')
        )

        if line_items_aggregate['total_printed'] == 0:
            if order_info.Bin:
                bin = order_info.Bin
                bin.Active = False
                bin.save()

            order_info.Bin = None
            order_info.save()

        all_items_printed = line_items_aggregate['total_printed'] >= line_items_aggregate['total_quantity']
        line_item.refresh_from_db()
        serializer = ReadLineItemSerializer(line_item)
        data = {
            "LineItem": serializer.data,
            "AllItemsPrinted": all_items_printed
        }

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_updated.delay(
                [l.Id for l in order_info.LineItems.all()])

        return response.Response(data)

    def post(self, request, pk=None):
        """
        docstring
        """
        line_item = LineItem.objects.get(Id=pk)

        line_items_aggregate = line_item.Order.LineItems.aggregate(
            total_quantity=Sum('Quantity'),
            total_printed=Sum('PrintedQuantity')
        )

        all_items_printed = line_items_aggregate['total_printed'] >= line_items_aggregate['total_quantity']

        if all_items_printed:
            serializer = ReadLineItemSerializer(line_item)
            data = {
                "LineItem": serializer.data,
                "AllItemsPrinted": all_items_printed
            }
            return response.Response(serializer.data)

        order_info = line_item.Order

        # Case 1, only one item, no need to assign Bin
        if line_items_aggregate['total_quantity'] <= 1 and not "Sydney Warehouse / Studio" in line_item.Shipping:
            pass
        else:
            if not order_info.Bin:
                bin = Bin.objects.exclude(
                    Number=0).filter(Active=False).first()
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
            ChangeStatus="Processed",
            LineItem=line_item
        )

        line_items_aggregate = line_item.Order.LineItems.aggregate(
            total_quantity=Sum('Quantity'),
            total_printed=Sum('PrintedQuantity')
        )

        all_items_printed = line_items_aggregate['total_printed'] >= line_items_aggregate['total_quantity']
        order_info.AllItemsPrinted = all_items_printed
        order_info.save()

        line_item.refresh_from_db()
        serializer = ReadLineItemSerializer(line_item)
        data = {
            "LineItem": serializer.data,
            "AllItemsPrinted": all_items_printed
        }

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_updated.delay(
                [l.Id for l in order_info.LineItems.all()])

        return response.Response(data)


class ResetDatabaseAPIView(views.APIView):
    """
    docstring
    """

    def post(self, request, *args, **kwargs):
        """
        docstring
        """
        reset_database_task.delay()
        return response.Response({"detail", "Database reset"})


class BinViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    serializer_class = BinSerializer
    filterset_fields = ["id",  "Number"]
    
    def get_serializer_class(self):
        if self.request.method == "GET":
            return ReadBinSerializer

        return self.serializer_class
    
    def get_queryset(self):
        return Bin.objects.filter(Active=True)
    
    def perform_update(self, serializer):
        instance = serializer.save()
        if settings.BROADCAST_ENABLED:
            tasks.broadcast.delay([instance.Number],"bins.updated")
        return instance
    


    # def list(self):
    #     active_bins = Bin.objects.filter(Active=True)
    #     serializer = ReadBinSerializer(active_bins, many=True)
        
    #     return response.Response(serializer.data)
        