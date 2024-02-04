import json
import logging
from uuid import uuid4
from django.shortcuts import render
from django.db.models import Sum, Count
from django_filters import rest_framework as filters
from rest_framework import filters as rest_framework_filters
from rest_framework.exceptions import APIException
from django.db.models import Count, F, Value
from tlkapi import tasks
from tlkapi.models import LineItem
from django.conf import settings
from rest_framework import pagination
import django_filters

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
    Bin,
    PrintRequest
)

from tools.models import (
    Variant
)

from .serializers import (
    BinSerializer,
    IdArraySerializer,
    PrintRequestSerializer,
    ReadLineItemSerializer,
    WriteLineItemSerializer,
    LogSerializer,
    OrderInfoSerializer,
    ReadBinSerializer
)
from .tasks import reset_database_task

logger = logging.getLogger(__name__)




def for_pickup(line_item:LineItem):
    """
    docstring
    """
    if not line_item or not line_item.Shipping:
        return False
    
    for for_pickup_shipping_line in settings.SHIPPING_LINES_FOR_PICKUP:
        if for_pickup_shipping_line in line_item.Shipping:
            return True
        
    return False

class LineItemViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    filterset_fields = ["Id", 'LineItemId', "OrderId", "OrderNumber", "Status"]
    pagination_class = pagination.LimitOffsetPagination


    def get_queryset(self):
        if self.request.method == 'GET':
            return LineItem.objects2.active_items().annotate(
                BinNumber=F('Order__Bin__Number'))
        else:
            return LineItem.objects.all()

    def get_serializer_class(self):
        if self.request.method == 'GET':
            return ReadLineItemSerializer
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
    
    @decorators.action(detail=True, methods=['post'])
    def request_print(self, request, pk=None):
        line_item = self.get_object()
        variant = Variant.objects.filter(shopify_id=line_item.VariantId).first()

        PrintRequest.objects.create(
            LineItem = line_item,
            Variant = variant
        )
        return response.Response({"success":True},status=status.HTTP_200_OK)
    
    @decorators.action(detail=True, methods=['post'])
    def set_status(self, request, pk=None):
        """
        docstring
        """
        new_status = request.POST.get('status')
        line_item = self.get_object()

        line_item.Status = new_status
        line_item.save()

        order = line_item.Order

        if new_status == "Archived":
            if order:
                all_items_archived = order.LineItems.filter(
                    Status='Archived').count() == order.LineItems.all().count()

                if all_items_archived and order.Bin:
                    tasks.archive_bin_task.delay(order.Bin.Number)

        if new_status == 'Pending':
            line_item.PrintedQuantity = 0
            line_item.save()

            if order:
                order.AllItemsPrinted = False
                order.LabelPrinted = False
                order.save()

        Log.objects.create(
            ChangeStatus=f"Updated status to '{new_status}'",
            LineItem=line_item
        )

        if settings.BROADCAST_ENABLED:
            tasks.broadcast_updated.delay([line_item.Id])

        return response.Response({
            "message": "Item's Status successfully updated"
        })


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
    filterset_fields = ["OrderId", 'Id', 'OrderNumber']



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

        tasks.archive_bin_task.delay(BinNumber)
        return response.Response({
            "message": "Bin emptied and items archived"
        })


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
            Log.objects.create(LineItem=line_item,
                               ChangeStatus="Minus-1 Print")

        if line_item.PrintedQuantity == 0:
            line_item.Status = "Pending"

        line_item.save()

        line_items_aggregate = LineItem.objects2.active_items().filter(Order=line_item.Order).aggregate(
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
            "AllItemsPrinted": all_items_printed,
            "BinNumber": 0
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

        line_items_aggregate = LineItem.objects2.active_items().filter(Order=line_item.Order).aggregate(
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
        if line_items_aggregate['total_quantity'] <= 1 and not for_pickup(line_item):
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
            ChangeStatus="Item Printed",
            LineItem=line_item
        )

        line_items_aggregate = LineItem.objects2.active_items().filter(Order=line_item.Order).aggregate(
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
            "AllItemsPrinted": all_items_printed,
        }

        if order_info.Bin:
            data["BinNumber"] = order_info.Bin.Number

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
            tasks.broadcast.delay([instance.Number], "bins.updated")
        return instance

    # def list(self):
    #     active_bins = Bin.objects.filter(Active=True)
    #     serializer = ReadBinSerializer(active_bins, many=True)

    #     return response.Response(serializer.data)


class ArchivedItemsListView(generics.ListAPIView):
    """
    docstring
    """
    queryset = LineItem.objects2.archived_items().annotate(
        OriginalBinNumber=F('Order__OriginalBinNumber')).order_by('-DateModified','-OrderNumber')
    serializer_class = ReadLineItemSerializer
    filter_backends = [filters.DjangoFilterBackend, rest_framework_filters.SearchFilter]
    filterset_fields = ["Id", 'LineItemId', "OrderId", "OrderNumber", "Status"]
    pagination_class = pagination.LimitOffsetPagination
    search_fields = ['OrderNumber','Sku','Name','VariantTitle','Shipping','Customer','CustomerEmail']

class RestoreArchived(views.APIView):
    """
    docstring
    """
    
    def post(self,request):
        """
        docstring
        """
        serializer = IdArraySerializer(data=request.data)
        if serializer.is_valid():
            ids = serializer.data.get('ids')
            archived_items = LineItem.objects2.archived_items().filter(Id__in=ids)
            archived_items.update(Status='Pending')
            if settings.BROADCAST_ENABLED:
                tasks.broadcast_added.delay(ids)
        return response.Response(serializer.errors)
    
class ConfigAPIView(views.APIView):
    """
    docstring
    """

    def get(self, request):
        data = {
            "logging_email":  settings.LOGGING_EMAIL,
            "logging_password":  settings.LOGGING_PASSWORD,
        }

        return response.Response(data)


class PrintAPIView(generics.ListCreateAPIView):
    """
    docstring
    """
    queryset = PrintRequest.objects.all()
    serializer_class = PrintRequestSerializer

    def list(self, request, *args, **kwargs):

        pr = PrintRequest.objects.filter(Done=False).first()
        if pr:
            pr.Done = True
            pr.save()

            serializer = PrintRequestSerializer([pr], many=True)
            return response.Response(serializer.data)
        else:
            return response.Response({}, status=status.HTTP_404_NOT_FOUND)
 