from .models import Hook
from rest_framework import serializers


class HookSerializer(serializers.ModelSerializer):
    class Meta:
        model = Hook
        fields = '__all__'
        read_only_fieds = ['headers']
