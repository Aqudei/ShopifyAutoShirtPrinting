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
from .views import ShopifyWebhookHandlerView

urlpatterns = [
    path('handler/9fbe8036806f4fe992552e9010d7fd07/',ShopifyWebhookHandlerView.as_view()),

    # path('handler/5ba7048b944f49ad8e1291698a7b46e5/',ShipStationWebhookHandlerView.as_view())
]