from django.apps import AppConfig


class TlkapiConfig(AppConfig):
    default_auto_field = 'django.db.models.BigAutoField'
    name = 'tlkapi'

    def ready(self):
        import tlkapi.signals