"""
URL configuration for pyscripts project.

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/4.2/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.urls import path
from rest_framework import routers
from .views import (
    LineItemViewSet,
    LogAPIView,
    OrderInfoViewSet,
    ListBinsView,
    AvailableBin,
    DestroyBinView,ResetDatabaseAPIView
)

router = routers.DefaultRouter()
router.register(r'LineItems', LineItemViewSet, basename='LineItem')
router.register(r'Orders', OrderInfoViewSet, basename='Order')

urlpatterns = router.urls
urlpatterns += [
    path('Logs/', LogAPIView.as_view()),
    path('Bins/<int:BinNumber>/', DestroyBinView.as_view()),
    path('Bins/', ListBinsView.as_view()),
    path('Bins/Available/', AvailableBin.as_view()),
    path('ResetDatabase/', ResetDatabaseAPIView.as_view()),
]