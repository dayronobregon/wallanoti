import os
from functools import lru_cache
from pathlib import Path

from app.application.entity_extractor.extract_entities import ExtractEntities
from app.infrastructure.nlp.spacy_ner_model import SpacyNERModel

# Modelos disponibles — configura MODEL_PATH en .env o como variable de entorno:
#
#   MODEL_PATH=models/ner_v1        → modelo blank entrenado desde cero
#   MODEL_PATH=models/ner_core_v1   → fine-tuning sobre es_core_news_md
_MODEL_PATH = Path(os.getenv("MODEL_PATH", "models/ner_core_v1"))


@lru_cache(maxsize=1)
def get_ner_model() -> SpacyNERModel:
    """
    Carga el modelo una sola vez (singleton via lru_cache).
    spaCy es costoso de inicializar — no queremos hacerlo en cada request.
    """
    return SpacyNERModel(model_path=_MODEL_PATH)


def get_extract_use_case() -> ExtractEntities:
    """
    Construye el caso de uso inyectando el modelo.
    FastAPI llama a esto en cada request (barato, solo ensambla objetos).
    """
    return ExtractEntities(ner_model=get_ner_model())
