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
        queryset = LineItem.objects.exclude(Status='Archived').annotate(
            BinNumber=F('Order__Bin__Number'))
        return queryset

    # queryset = LineItem.objects.exclude(Status='Archived')
    serializer_class = ReadLineItemSerializer

    def get_serializer_class(self):
        if self.request.method == 'GET':
            return self.serializer_class
        else:
            return WriteLineItemSerializer

    def perform_create(self, serializer):

        order_number = serializer.validated_data.get("OrderNumber")

        if order_number:
            order = OrderInfo.objects.get(OrderNumber=order_number)
            sample_line = LineItem.objects.filter(Order=order).first()
            instance = serializer.save(Order=order, Customer=sample_line.Customer,
                                       CustomerEmail=sample_line.CustomerEmail, 
                                       Shipping=sample_line.Shipping)
            order.AllItemsPrinted = False
            order.save()
        else:
            order = OrderInfo.objects.create(
                OrderNumber=str(uuid4()).split("-")[0])
            instance = serializer.save(Order=order, OrderNumber=order.OrderNumber)

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_added.delay([instance.Id])

        return instance

    def perform_update(self, serializer):
        instance = serializer.save()

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_updated.delay([instance.Id])

        return instance

    filterset_fields = ["Id", 'LineItemId', "OrderId", "OrderNumber"]


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
                "OrderNumber": order.OrderNumber,
                "BinNumber": order.Bin.Number,
                "LineItems": serializer.data,
                "Notes": order.Bin.Notes
            })

        return response.Response(data)