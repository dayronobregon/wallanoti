from pathlib import Path

import spacy

from app.domain.entity_extractor import NerExtractor
from app.domain.entity_model import EntityModel, EntityType
from app.domain.extraction_model import ExtractionModel

_LABEL_TO_ENTITY_TYPE: dict[str, EntityType] = {
    "PRODUCT": EntityType.Product,
    "LOCATION": EntityType.Location,
    "MIN_PRICE": EntityType.MinPrice,
    "MAX_PRICE": EntityType.MaxPrice,
}


def _make_entity(label: str, value: str) -> EntityModel:
    entity = EntityModel(classification=_LABEL_TO_ENTITY_TYPE[label], value=value)

    return entity


class SpacyNERModel(NerExtractor):
    """
    Implementación concreta de NerExtractor usando spaCy.

    Es la ÚNICA clase del proyecto que conoce spaCy.
    Si cambias de modelo, solo tocas este archivo.
    """

    def __init__(self, model_path: Path) -> None:
        if not model_path.exists():
            raise FileNotFoundError(
                f"No se encontró el modelo en: {model_path}. "
                "¿Ejecutaste el entrenamiento?"
            )
        self._nlp = spacy.load(model_path)

    def extract(self, text: str) -> ExtractionModel:
        doc = self._nlp(text)

        found: dict[str, EntityModel] = {}
        for ent in doc.ents:
            if ent.label_ in _LABEL_TO_ENTITY_TYPE:
                found[ent.label_] = _make_entity(ent.label_, ent.text)

        return ExtractionModel(
            product=found.get("PRODUCT"),
            location=found.get("LOCATION"),
            min_price=found.get("MIN_PRICE"),
            max_price=found.get("MAX_PRICE"),
        )
