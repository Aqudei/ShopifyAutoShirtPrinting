from django.shortcuts import render
from django.db.models import Sum, Count
from django_filters import rest_framework as filters

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
from .models import LineItem, Log, OrderInfo
from .serializers import (
    LineItemSerializer,
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
    queryset = LineItem.objects.exclude(Status='Archived')
    serializer_class = LineItemSerializer
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
        serializer = LineItemSerializer(queryset,many=True)
        
        return response.Response(serializer.data)


class OrderInfoViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = OrderInfo.objects.all()

    serializer_class = OrderInfoSerializer
    filterset_fields = ['BinNumber', "OrderId", "Active"]


class LogAPIView(generics.ListCreateAPIView):
    queryset = Log.objects.all()
    serializer_class = LogSerializer
    filterset_fields = ['LineItem']

class DestroyBinView(views.APIView):
    """
    docstring
    """
    def post(self,request):
        """
        docstring
        """
        orders = OrderInfo.objects.filter(BinNumber=self.kwargs['BinNumber'])
        order_ids = []
        for order in orders:
            order.Active = False
            order.BinNumber = 0
            order.save()

            order_ids.append(order.OrderId)

        line_items = LineItem.objects.filter(OrderId__in=order_ids)
        for line_item in line_items:
            line_item.BinNumber = 0
            line_item.save()


class ProcessItemView(views.APIView):
    """
    docstring
    """
    def post(self,request, pk=None):
        """
        docstring
        """
        bin_number_response = None
        line_item = LineItem.objects.get(pk=pk)
        line_items_aggregate = line_item.Order.LineItems.aggregate(total_quantity= Sum('Quantity'))
        order_info = line_item.Order

        # Case 1, only one item, no need to assign Bin
        if line_items_aggregate['total_quantity'] <= 1:

            bin_number_response = {
                "BinNumber" : 0
            }

        # Case 2, Some items already in the Bin
        elif line_item.Order.Active:
            bin_number_response = {
                "BinNumber" : order_info.BinNumber
            }
        
        else:
            # Case 3, first item of many, assigned to Bin
            for i in range(50):
                if i==0:
                    continue

                orders = OrderInfo.objects.filter(BinNumber=i, Active=True) 
                if orders.exists():
                    continue

                bin_number_response = {
                    "BinNumber" : i
                }
                break

        order_info = line_item.Order
        order_info.Active = True
        order_info.BinNumber = bin_number_response['BinNumber']
        order_info.save()
        
        line_item.Status = "Processed"
        line_item.PrintedQuantity += 1
        line_item.save()

        LineItem.objects.filter(OrderId=order_info.OrderId).update(
            BinNumber = bin_number_response['BinNumber']
        )
        
        Log.objects.create(
            ChangeStatus = "Processed",
            LineItem = line_item
        )

        line_item.refresh_from_db()
        serializer = LineItemSerializer(line_item)
        return response.Response(serializer.data)
    

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
        active_orders = OrderInfo.objects.filter(BinNumber__gte=1, Active=True)
        print(active_orders)
        bins = []
        for order in active_orders:
            line_items = LineItem.objects.filter(OrderId=order.OrderId)
            bins.append({
                "BinNumber": order.BinNumber,
                "OrderNumber": line_items[0].OrderNumber,
                "Items" : LineItemSerializer(line_items, many=True).data
            })
        serializer = self.serializer_class(data=bins, many=True)
        if serializer.is_valid():
            return response.Response(serializer.data)

        return response.Response(serializer.errors, status=status.HTTP_200_OK)
