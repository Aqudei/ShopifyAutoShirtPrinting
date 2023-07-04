from django.shortcuts import render
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

# Create your views here.


class LineItemViewSet(viewsets.ModelViewSet):
    """
    docstring
    """
    queryset = LineItem.objects.exclude(Status='Archived')
    serializer_class = LineItemSerializer
    filterset_fields = ['LineItemId', "OrderId"]


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
    filterset_fields = ['MyLineItemId']

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


class AvailableBin(views.APIView):
    """
    docstring
    """
    def get(self,request):
        """
        docstring
        """
        for i in range(200):
            if i==0:
                continue

            orders = OrderInfo.objects.filter(BinNumber=i, Active=True) 
            if orders.exists():
                continue

            return response.Response({
                "AvailableBinNumber" : i
            })



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
